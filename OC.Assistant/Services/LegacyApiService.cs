using System.IO.Pipes;
using System.Text;
using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Services;

/// <summary>
/// The legacy API for the sake of backwards compatibility.
/// Is used to receive remote messages via pipe stream.
/// </summary>
internal class LegacyApiService : IDisposable
{
    private readonly CancellationTokenSource _cancel = new();
    
    public LegacyApiService()
    {
        Task.Run(ListenAsync);
    }
    
    private async Task ListenAsync()
    {
        while (true)
        {
            var token = _cancel.Token;
            if (token.IsCancellationRequested) return;
            var pipeServer = CreatePipeServer();
            await pipeServer.WaitForConnectionAsync(token);
            var size = await ReadExpectedSizeAsync(pipeServer, token);
            var payload = await ReadPayloadAsync(pipeServer, size, token);
            try
            {
                HandleMessage(payload);
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
            }
            pipeServer.Close();
        }
    }

    private static NamedPipeServerStream CreatePipeServer()
    {
        return new NamedPipeServerStream(
            "OC.Assistant.server", 
            PipeDirection.InOut, 
            1, 
            PipeTransmissionMode.Byte, 
            PipeOptions.Asynchronous);
    }
    
    private static async Task<int> ReadExpectedSizeAsync(PipeStream pipeServer, CancellationToken token)
    {
        var header = new byte[4];
        var result = await pipeServer.ReadAsync(header.AsMemory(0, header.Length), token);
        var isValid = header[3] == (byte) (header[2] + (header[0] ^ header[1]));
        return result == header.Length && isValid ? header[0] << 8 | header[1] : 0;
    }
    
    private static async Task<byte[]?> ReadPayloadAsync(PipeStream pipeServer, int size, CancellationToken token)
    {
        if (size <= 0) return null;
        var payload = new byte[size];
        var result = await pipeServer.ReadAsync(payload.AsMemory(0, payload.Length), token);
        return result == payload.Length ? payload : null;
    }
    
    private static void HandleMessage(byte[]? payload)
    {
        if (payload is null || payload.Length <= 3) return;
        var str = Encoding.UTF8.GetString(payload, 0, payload.Length);
        var type = str[..3];
        var data = str[3..];

        switch (type)
        {
            case "cfg":
                EventSystem.InvokeApiEvent("data/config", new XElement("Payload", XElement.Parse(data)));
                break;
            case "tsc":
                EventSystem.InvokeApiEvent("data/timeScaling", new XElement("Payload", data));
                break;
        }
    }
    
    public void Dispose()
    {
        _cancel.Cancel();
    }
}