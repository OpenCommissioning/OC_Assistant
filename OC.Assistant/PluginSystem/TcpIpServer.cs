using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OC.Assistant.PluginSystem;

public static class TcpIpServer
{
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
        var appSettings = AppControl.Instance.Settings;
        
        var listener = new TcpListener(IPAddress.TryParse(appSettings.IpAddress, out var address) ? 
            address : 
            IPAddress.Any, appSettings.PluginServerPort);
        
        listener.Start();
        Sdk.Logger.LogInfo(typeof(TcpIpServer), $"PluginServer listening on {address}:{appSettings.PluginServerPort}");
        
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
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken token = default)
    {
        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[4096];

            while (!token.IsCancellationRequested)
            {
                if (!await ReadAsync(stream, buffer, 4, token)) break;
                var channelLength = BitConverter.ToInt32(buffer.AsSpan()[..4]);

                if (!await ReadAsync(stream, buffer, channelLength, token)) break;
                var channel = Encoding.UTF8.GetString(buffer, 0, channelLength);
                
                if (!await ReadAsync(stream, buffer, 4, token)) break;
                var payloadLength = BitConverter.ToInt32(buffer.AsSpan()[..4]);

                var payload = new byte[payloadLength];
                if (!await ReadAsync(stream, payload, payloadLength, token)) break;

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
                
                var responseLength = BitConverter.GetBytes(writeBuffer.Length);
                await stream.WriteAsync(responseLength, token);
                await stream.WriteAsync(writeBuffer, token);
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
                    await stream.WriteAsync(BitConverter.GetBytes(readRequest.Index), token);
                    await stream.WriteAsync(BitConverter.GetBytes(readRequest.CbLength), token);
                    return;
                case 2: //WR_REC
                    if (RecordData.Instance.TryGetWriteRequest(identifier, hardwareId) is not {} writeRequest) break;
                    if (writeRequest.Data?.Length != writeRequest.CbLength) break;
                    await stream.WriteAsync(BitConverter.GetBytes(writeRequest.Index), token);
                    await stream.WriteAsync(BitConverter.GetBytes(writeRequest.CbLength), token);
                    await stream.WriteAsync(writeRequest.Data, token);
                    return;
                case 3: //RD_RES
                    index = BitConverter.ToUInt16(payload.AsSpan()[6..]);
                    dataLength = BitConverter.ToUInt32(payload.AsSpan()[8..]);
                    RecordData.Instance.SendReadRes(identifier, hardwareId, index, dataLength, payload[14..]);
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