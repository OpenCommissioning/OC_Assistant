using System.IO.Pipes;
using System.Xml.Linq;

namespace OC.Assistant.Core;

internal class ApiNode : IDisposable
{
    private static readonly Lazy<ApiNode> LazyInstance = new(() => new ApiNode());
    private readonly CancellationTokenSource _cancel = new();
    private NamedPipeServerStream? _pipeServer;
    
    /// <summary>
    /// Singleton interface for the <see cref="ApiNode"/>.
    /// </summary>
    public static ApiNode Interface => LazyInstance.Value;
    
    /// <summary>
    /// Is raised when a config has been received.
    /// </summary>
    public event Action<XElement>? ConfigReceived;
    
    /// <summary>
    /// Is raised when the TimeScaling value has been changed.
    /// </summary>
    public event Action<double>? TimeScalingReceived;

    private ApiNode()
    {
        if (LazyInstance.IsValueCreated) return;
        Listen();
    }

    private void Listen()
    {
        var cancelToken = _cancel.Token;
        
        Task.Run(async () =>
        {
            while (!cancelToken.IsCancellationRequested)
            {
                _pipeServer = new NamedPipeServerStream(
                    "OC.Assistant.server", 
                    PipeDirection.InOut, 
                    1, 
                    PipeTransmissionMode.Byte, 
                    PipeOptions.Asynchronous);
                
                await _pipeServer.WaitForConnectionAsync(_cancel.Token);
                
                var buffer = new byte[ApiMessage.HEADER_SIZE];
                
                if (await _pipeServer.ReadAsync(buffer.AsMemory(0, ApiMessage.HEADER_SIZE), cancelToken) == 
                    ApiMessage.HEADER_SIZE && ApiMessage.IsHeaderValid(buffer))
                {
                    var size = ApiMessage.GetExpectedSize(buffer);
                    
                    buffer = new byte[size];

                    if (await _pipeServer.ReadAsync(buffer.AsMemory(0, size), cancelToken) == size)
                    {
                        await HandleMessage(new ApiMessage(buffer));
                    }
                }

                _pipeServer?.Close();
            }
        }, cancelToken);
    }
    
    private async Task HandleMessage(ApiMessage message)
    {
        var str = message.ToString();
        var type = str[..3];
        var msg = str.Remove(0, 3);

        switch (type)
        {
            case "cfg":
                ConfigReceived?.Invoke(XElement.Load(msg));
                break;
            case "tsc":
                TimeScalingReceived?.Invoke(double.TryParse(msg, out var value) ? value : 1.0);
                break;
        }
    }

    public void Dispose()
    {
        _cancel.Cancel();
        _pipeServer?.Close();
    }
}