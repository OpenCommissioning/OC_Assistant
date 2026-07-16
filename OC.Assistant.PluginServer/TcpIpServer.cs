using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OC.Assistant.PluginServer;

public static class TcpIpServer
{
    public static string DefaultIpAddress => "127.0.0.1";
    public static int DefaultPort => 50100;
    private static CancellationTokenSource? _cts;
    private static readonly List<Task> Tasks = [];

    public static void RunDetached()
    {
        if (_cts is not null) return;
        _cts = new CancellationTokenSource();
        Tasks.Add(RunAsync(_cts.Token)); 
    }

    private static async Task RunAsync(CancellationToken token)
    {
        var ipAddress = IPAddress.TryParse(Sdk.XmlFile.Instance.IpAddress, out var value) ? 
            value : IPAddress.Any;
        
        var listener = new TcpListener(ipAddress, Sdk.XmlFile.Instance.Port);
        
        listener.Start();
        Sdk.Logger.LogInfo(typeof(TcpIpServer), $"PluginServer listening on {listener.LocalEndpoint}");
        
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync(token);
                    Tasks.Add(HandleClientAsync(client, token));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
        finally
        {
            listener.Stop();
        }
    }
    
    public static async Task CloseAsync()
    {
        if (_cts is null) return;
        await _cts.CancelAsync();
        await Task.WhenAll(Tasks);
        Tasks.Clear();
        _cts.Dispose();
        _cts = null;
        Sdk.Logger.LogInfo(typeof(TcpIpServer), "PluginServer stopped");
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken token = default)
    {
        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[8];

            while (!token.IsCancellationRequested)
            {
                if (!await ReadAsync(stream, buffer, 8, token)) break;
                var channelLength = BitConverter.ToInt32(buffer.AsSpan()[..4]);
                var payloadLength = BitConverter.ToInt32(buffer.AsSpan()[4..8]);

                string? channel = null;
                byte[]? payload = null;

                await WithSharedBuffer(channelLength + payloadLength, async (l, b) =>
                {
                    if (!await ReadAsync(stream, b, l, token)) return;
                    channel = Encoding.UTF8.GetString(b, 0, channelLength);
                    payload = new byte[payloadLength];
                    Buffer.BlockCopy(b, channelLength, payload, 0, payloadLength);
                });

                if (string.IsNullOrEmpty(channel) || payload is null) break;

                if (channel == "/R")
                {
                    await HandleRecordDataAsync(stream, payload, token);
                    continue;
                }
                
                if (TcpIpChannel.ReadBuffers.TryGetValue(channel, out var readBuffer) && readBuffer.Length == payload.Length)
                {
                    Array.Copy(payload, readBuffer, payload.Length);
                }
                
                if (!TcpIpChannel.WriteBuffers.TryGetValue(channel, out var writeBuffer))
                {
                    await stream.WriteAsync(BitConverter.GetBytes(0), token);
                    continue;
                }

                await WithSharedBuffer(4 + writeBuffer.Length, async (l, b) =>
                {
                    BinaryPrimitives.WriteInt32LittleEndian(b, writeBuffer.Length);
                    Buffer.BlockCopy(writeBuffer, 0, b, 4, writeBuffer.Length);
                    await stream.WriteAsync(b.AsMemory()[..l], token);
                });
            }
        }
        catch (Exception e)
        {
            Sdk.Logger.LogWarning(typeof(TcpIpServer), e.Message);
        }
        finally
        {
            client.Close();
        }
    }
    
    private static async Task<bool> ReadAsync(NetworkStream stream, byte[] buffer, int length, CancellationToken token = default)
    {
        var read = 0;
        while (read < length)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(read, length - read), token);
            if (bytesRead == 0)
                return false;
            read += bytesRead;
        }
        return true;
    }

    private static async Task WithSharedBuffer(int length, Func<int, byte[], Task> func)
    {
        var rented = ArrayPool<byte>.Shared.Rent(length);
        
        try
        {
            await func.Invoke(length, rented);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
    
    private static async Task HandleRecordDataAsync(NetworkStream stream, byte[] payload, CancellationToken token = default)
    {
        try
        {
            var command = BitConverter.ToUInt16(payload);
            var hardwareId = BitConverter.ToUInt16(payload.AsSpan()[2..]);
            var identifier = BitConverter.ToUInt16(payload.AsSpan()[4..]);
            ushort index;
            uint dataLength;
            
            RecordData.Instance.Subscribe(identifier, hardwareId);

            switch (command)
            {
                case 1: //RD_REC
                    if (RecordData.Instance.TryGetReadRequest(identifier, hardwareId) is not {} readRequest) break;
                    await WithSharedBuffer(6, async (l, b) =>
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(b, readRequest.Index);
                        BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan()[2..], readRequest.CbLength);
                        await stream.WriteAsync(b.AsMemory()[..l], token);
                    });
                    return;
                case 2: //WR_REC
                    if (RecordData.Instance.TryGetWriteRequest(identifier, hardwareId) is not {} writeRequest) break;
                    if (writeRequest.Data?.Length != writeRequest.CbLength) break;
                    await WithSharedBuffer(6 + writeRequest.Data.Length, async (l, b) =>
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(b, writeRequest.Index);
                        BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan()[2..], writeRequest.CbLength);
                        Buffer.BlockCopy(writeRequest.Data, 0, b, 6, writeRequest.Data.Length);
                        await stream.WriteAsync(b.AsMemory()[..l], token);
                    });
                    return;
                case 3: //RD_RES
                    index = BitConverter.ToUInt16(payload.AsSpan()[6..]);
                    dataLength = BitConverter.ToUInt32(payload.AsSpan()[8..]);
                    RecordData.Instance.SendReadRes(identifier, hardwareId, index, dataLength, payload[12..]);
                    break;
                case 4: //WR_RES
                    index = BitConverter.ToUInt16(payload.AsSpan()[6..]);
                    dataLength = BitConverter.ToUInt32(payload.AsSpan()[8..]);
                    RecordData.Instance.SendWriteRes(identifier, hardwareId, index, dataLength);
                    break;
            }
        }
        catch (Exception e)
        {
            Sdk.Logger.LogWarning(typeof(TcpIpServer), e.Message);
        }
        
        await stream.WriteAsync(new byte[1], token);
    }
}