using System.IO.Pipes;
using System.Text;
using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Api;

/// <summary>
/// Represents an API based on a named pipe stream.
/// Is used to receive remote messages.
/// </summary>
internal static class NamedPipeApi
{
    private static CancellationTokenSource? _cts;
    
    public static void RunDetached()
    {
        if (_cts is not null) return;
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenAsync(_cts.Token));
    }
    
    private static async Task ListenAsync(CancellationToken token)
    {
        while (true)
        {
            try
            {
                if (token.IsCancellationRequested) return;
                var pipeServer = CreatePipeServer();
                await pipeServer.WaitForConnectionAsync(token);
                var size = await ReadExpectedSizeAsync(pipeServer, token);
                var payload = await ReadPayloadAsync(pipeServer, size, token);
                HandleMessage(payload);
                pipeServer.Close();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(NamedPipeApi), e.Message);
            }
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
        var msg = str[3..];

        switch (type)
        {
            case "cfg":
                if (BusyState.IsSet) return;
                AppControl.Instance.UpdateConfig(XElement.Load(msg));
                break;
            case "tsc":
                AppControl.Instance.UpdateTimeScaling(double.TryParse(msg, out var value) ? value : 1.0);
                break;
        }
    }

    public static async Task CloseAsync()
    {
        if (_cts is null) return;
        await _cts.CancelAsync();
        _cts.Dispose();
        _cts = null;
    }
}