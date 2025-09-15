using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OC.Assistant.Plugins;

public class TcpIpServer
{
    private CancellationTokenSource? _cts;
    private readonly List<Task> _tasks = [];

    public void RunDetached(string ipAddress, int port)
    {
        _tasks.Add(RunAsync(ipAddress, port)); 
    }

    private async Task RunAsync(string ipAddress, int port)
    {
        if (_cts is not null) await CloseAsync();
        _cts = new CancellationTokenSource();

        var token = _cts.Token;
        var listener = new TcpListener(IPAddress.TryParse(ipAddress, out var address) ? address : IPAddress.Any, port);
        listener.Start();
        Sdk.Logger.LogInfo(this, $"TcpIpServer listening on {ipAddress}:{port}");
        
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync(token);
                    _tasks.Add(HandleClientAsync(client, token));
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
    
    public async Task CloseAsync()
    {
        if (_cts is null) return;
        await _cts.CancelAsync();
        await Task.WhenAll(_tasks);
        _tasks.Clear();
        _cts.Dispose();
        _cts = null;
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token = default)
    {
        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[4096];

            while (!token.IsCancellationRequested)
            {
                if (!await ReadExactAsync(stream, buffer, 4, token)) break;
                var channelLength = BitConverter.ToInt32(buffer.AsSpan()[..4]);

                if (!await ReadExactAsync(stream, buffer, channelLength, token)) break;
                var channel = Encoding.UTF8.GetString(buffer, 0, channelLength);
                
                if (!await ReadExactAsync(stream, buffer, 4, token)) break;
                var payloadLength = BitConverter.ToInt32(buffer.AsSpan()[..4]);

                var payload = new byte[payloadLength];
                if (!await ReadExactAsync(stream, payload, payloadLength, token)) break;

                if (channel == "/R")
                {
                    await HandleRecordData(stream, payload, token);
                    continue;
                }
                
                if (MemoryClient.ReadBuffers.TryGetValue(channel, out var readBuffer) && readBuffer.Length == payload.Length)
                {
                    Array.Copy(payload, readBuffer, payload.Length);
                }
                
                if (!MemoryClient.WriteBuffers.TryGetValue(channel, out var writeBuffer))
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
            Sdk.Logger.LogWarning(this, e.Message);
        }
        finally
        {
            client.Close();
        }
    }
    
    private static async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int length, CancellationToken token = default)
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
    
    private async Task HandleRecordData(NetworkStream stream, byte[] payload, CancellationToken token = default)
    {
        var direction = payload[0];
        if (direction is < 1 or > 4) return;
        
        var identifier = BitConverter.ToUInt32(payload.AsSpan()[2..]);
        uint index = 0;
        uint dataLength = 0;

        switch (direction)
        {
            case 1: //RD_REC
                //get from RD_REC dict via identifier
                //...
                await stream.WriteAsync(BitConverter.GetBytes(index), token);
                await stream.WriteAsync(BitConverter.GetBytes(dataLength), token);
                break;
            case 2: //WR_REC
                //get from WR_REC dict via identifier
                //...
                var data = Array.Empty<byte>();
                if (data.Length != dataLength) return;
                await stream.WriteAsync(BitConverter.GetBytes(index), token);
                await stream.WriteAsync(BitConverter.GetBytes(dataLength), token);
                await stream.WriteAsync(data, token);
                break;
            case 3: //RD_RES
                // invoke ReadRes
                //...
                await stream.WriteAsync(new byte[1], token);
                break;
            case 4: //WR_RES
                // invoke WriteRes
                //...
                await stream.WriteAsync(new byte[1], token);
                break;
        }
    }
}
