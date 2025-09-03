using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OC.Assistant.Core;

public class TcpServer
{
    private CancellationTokenSource? _cts;
    
    public async Task RunAsync(IPAddress ipAddress, int port)
    {
        if (_cts is not null) await CloseAsync();
        _cts = new CancellationTokenSource();
        
        var token = _cts.Token;
        var listener = new TcpListener(ipAddress, port);
        listener.Start();
        Sdk.Logger.LogInfo(this, $"TcpServer listening on {ipAddress}:{port}");

        while (!token.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(token);
            _ = HandleClientAsync(client, token);
        }
    }
    
    public async Task CloseAsync()
    {
        if (_cts is null) return;
        await _cts.CancelAsync();
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
                var channelLength = BitConverter.ToInt32(buffer[..4].Reverse().ToArray());

                if (!await ReadExactAsync(stream, buffer, channelLength, token)) break;
                var channel = Encoding.UTF8.GetString(buffer, 0, channelLength);
                
                if (!await ReadExactAsync(stream, buffer, 4, token)) break;
                var payloadLength = BitConverter.ToInt32(buffer[..4].Reverse().ToArray());

                var payload = new byte[payloadLength];
                if (!await ReadExactAsync(stream, payload, payloadLength, token)) break;
                
                if (Sdk.MemoryClient.ReadBuffers.TryGetValue(channel, out var readBuffer) && readBuffer.Length == payload.Length)
                {
                    Array.Copy(payload, readBuffer, payload.Length);
                }
                
                if (!Sdk.MemoryClient.WriteBuffers.TryGetValue(channel, out var writeBuffer))
                {
                    await stream.WriteAsync(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(0)), token);
                    continue;
                }
                
                var responseLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(writeBuffer.Length));
                await stream.WriteAsync(responseLength, token);
                await stream.WriteAsync(writeBuffer, token);
            }
        }
        catch (Exception e)
        {
            Sdk.Logger.LogError(this, e.Message);
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
}
