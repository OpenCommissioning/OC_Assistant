using System.Net;
using System.Net.Sockets;

namespace OC.Assistant.Sdk.TcpIp;

/// <summary>
/// Tcp/Ip Server.
/// </summary>
public class Server : TcpListener
{
    private CancellationTokenSource? _cancellationTokenSource;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly Message _message = new();
    private readonly int _timeoutMilliseconds;
    private bool IsConnected => _client?.Connected ?? false;
    private bool _isConnecting;

    /// <summary>
    /// An error occured.
    /// </summary>
    public event ErrorHandler? OnError;
    
    /// <summary>
    /// The server received a message from the client.
    /// </summary>
    public event MessageHandler? OnClientMessage;
    
    /// <summary>
    /// A new client has been connected to the server.
    /// </summary>
    public event ConnectionHandler? OnConnected;
    
    /// <summary>
    /// The port the server is listening on.
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// Creates a new instance of the Tcp/Ip server.
    /// </summary>
    /// <param name="ipAddress">IP address. Can also be <see cref="IPAddress.Any"/>.</param>
    /// <param name="port">The port the server is listening on.</param>
    /// <param name="timeoutMilliseconds">The timeout for receiving messages from the client.</param>
    public Server(IPAddress ipAddress, int port, int timeoutMilliseconds = 5000) : base(ipAddress, port)
    {
        Port = port;
        _timeoutMilliseconds = timeoutMilliseconds;
    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    public new void Start()
    {
        if (Active || _isConnecting) return;
            
        _cancellationTokenSource = new CancellationTokenSource();
        base.Start();
            
        var token = _cancellationTokenSource.Token;

        Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                if (!WaitForConnection(token)) continue;
                Receive(token);
            }
        }, token);
    }

    /// <summary>
    /// Stops the server and closes the stream.
    /// </summary>
    public new void Stop()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _stream?.Close();
            _client?.Close();
            base.Stop();
        }
        catch (Exception e)
        {
            OnError?.Invoke(e.Message);
        }
    }
        
    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    public void Send(byte[] message)
    {
        if (!IsConnected)
        {
            OnError?.Invoke("No client connected");
        }
            
        try
        {
            _message.Buffer = message;
            _stream?.Write(_message);
        }
        catch(Exception e)
        {
            OnError?.Invoke(e.Message);
        }
    }

    private bool WaitForConnection(CancellationToken token)
    {
        _isConnecting = true;
        try
        {
            while (!token.IsCancellationRequested && !IsConnected)
            {
                if (!Pending())
                {
                    Thread.Sleep(500);
                    continue;
                }
                _client = AcceptTcpClient();
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke(e.Message);
            _isConnecting = false;
            return false;
        }

        if (_client?.Client.RemoteEndPoint is null)
        {
            return false;
        }
        _isConnecting = false;
        if (!IsConnected) return false;
        _client.ReceiveTimeout = _timeoutMilliseconds;
        _stream = _client.GetStream();
        OnConnected?.Invoke(_client.Client.RemoteEndPoint);
        return true;
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
                OnClientMessage?.Invoke(_message.Buffer, _message.Length);
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