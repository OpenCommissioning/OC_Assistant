using System.IO.Pipes;

namespace OC.Assistant.Sdk;

internal class ApiNode(string localPipeName, string remotePipeName, string remoteHostname) : IDisposable
{
    private NamedPipeServerStream? _pipeServer;
    private readonly CancellationTokenSource _cancel = new();

    public void Listen()
    {
        Listen($"{localPipeName}.server");
    }

    public Action<ApiMessage>? MessageReceived;
    
    public async Task<bool> Send(string data)
    {
        try
        {
            using var pipeStream = new NamedPipeClientStream(
                remoteHostname, 
                $"{remotePipeName}.server", 
                PipeDirection.InOut, 
                PipeOptions.Asynchronous);
            await pipeStream.ConnectAsync(100);
            var apiMessage = new ApiMessage(data);
            await pipeStream.WriteAsync(apiMessage.Buffer, 0, apiMessage.Buffer.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void Listen(string pipeName)
    {
        var cancelToken = _cancel.Token;
        
        Task.Run(async () =>
        {
            while (!cancelToken.IsCancellationRequested)
            {
                _pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await _pipeServer.WaitForConnectionAsync(_cancel.Token);
                
                var buffer = new byte[ApiMessage.HeaderSize];
                if (await _pipeServer.ReadAsync(buffer, 0, ApiMessage.HeaderSize, cancelToken) == ApiMessage.HeaderSize 
                    && ApiMessage.IsHeaderValid(buffer))
                {
                    var size = ApiMessage.GetExpectedSize(buffer);
                    buffer = new byte[size];

                    if (await _pipeServer.ReadAsync(buffer, 0, size, cancelToken) == size)
                    {
                        MessageReceived?.Invoke(new ApiMessage(buffer));
                    }
                }

                _pipeServer?.Close();
            }
        }, cancelToken);
    }

    public void Dispose()
    {
        _cancel.Cancel();
        _pipeServer?.Close();
    }
}