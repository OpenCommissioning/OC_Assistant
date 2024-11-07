using System.Net.Sockets;

namespace OC.Assistant.Sdk.TcpIp;

/// <summary>
/// Tcp/Ip Client.
/// </summary>
public class Client
{
    private readonly Message _message = new();
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string _hostName;
    private readonly int _port;
    private readonly int _timeoutMilliseconds;
    private bool IsConnected => _client?.Connected ?? false;
    private bool _isConnecting;

    /// <summary>
    /// An error occured.
    /// </summary>
    public event ErrorHandler? OnError;
    
    /// <summary>
    /// The client received a message from the server.
    /// </summary>
    public event MessageHandler? OnServerMessage;
    
    /// <summary>
    /// The client has been connected to the server.
    /// </summary>
    public event Action? OnConnected;
        
    /// <summary>
    /// Creates a new instance of the Tcp/Ip client.
    /// </summary>
    /// <param name="hostName">The server hostname.</param>
    /// <param name="port">The port where the server is listening.</param>
    /// <param name="timeoutMilliseconds">The timeout for receiving messages from the server.</param>
    public Client(string hostName, int port, int timeoutMilliseconds = 5000)
    {
        _hostName = hostName;
        _port = port;
        _timeoutMilliseconds = timeoutMilliseconds;
    }

    /// <summary>
    /// Connects to the server.
    /// </summary>
    public void Start()
    {
        if (IsConnected || _isConnecting) return;
            
        _cancellationTokenSource = new CancellationTokenSource();
            
        var token = _cancellationTokenSource.Token;
            
        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                if (!WaitForConnection(token)) return;
                Receive(token);
            }
        }, token);
    }
        
    /// <summary>
    /// Closes the client and the stream.
    /// </summary>
    public void Stop()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _stream?.Close();
            _client?.Close();
        }
        catch (Exception e)
        {
            OnError?.Invoke(e.Message);
        }
    }

    private bool WaitForConnection(CancellationToken token)
    {
        _isConnecting = true;
        while (!token.IsCancellationRequested && !IsConnected)
        {
            try
            {
                _client = new TcpClient(_hostName, _port);
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
                Thread.Sleep(2000);
            }
        }

        if (_client is null)
        {
            return false;
        }
        _isConnecting = false;
        if (!IsConnected) return false;
        _client.ReceiveTimeout = _timeoutMilliseconds;
        _stream = _client.GetStream();
        OnConnected?.Invoke();
        return true;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    public void Send(byte[] message)
    {
        if (!IsConnected) return;

        try
        {
            _message.Buffer = message;
            _stream?.Write(_message);
        }
        catch (Exception e)
        {
            OnError?.Invoke(e.Message);
        }
    }
        
    private void Receive(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!IsConnected) break;
                _stream?.Read(_message);
                if (_message.Length == 0) break;
                OnServerMessage?.Invoke(_message.Buffer, _message.Length);
            }
            catch(Exception e)
            {
                OnError?.Invoke(e.Message);
                break;
            }
        }

        try
        {
            _stream?.Close();
            _client?.Close();
        }
        catch(Exception e)
        {
            OnError?.Invoke(e.Message);
        }
    }
}