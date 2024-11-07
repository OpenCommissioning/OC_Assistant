using System.Xml.Linq;

namespace OC.Assistant.Sdk;

/// <summary>
/// Represents the local implementation of the Assistant Api.
/// </summary>
public class ApiLocal
{
    private const string CONFIG_IDENTIFIER = "cfg";
    private const string MESSAGE_IDENTIFIER = "msg";
    private const string TIMESCALE_IDENTIFIER = "tsc";

    private readonly ApiNode? _node;
    private static readonly Lazy<ApiLocal> LazyInstance = new(() => new ApiLocal());

    /// <summary>
    /// Private singleton constructor for the <see cref="ApiLocal"/> class.
    /// </summary>
    private ApiLocal()
    {
        if (LazyInstance.IsValueCreated) return;
        
        _node = new ApiNode(
            "OC.Assistant", 
            "OC.Assistant.Remote", 
            ".");
        
        _node.MessageReceived += OnReceived;
        _node.Listen();
    }
    
    /// <summary>
    /// Singleton interface for the <see cref="ApiLocal"/>.
    /// </summary>
    public static ApiLocal Interface => LazyInstance.Value;
    
    /// <summary>
    /// Updates the Sil plugin by the given name.
    /// </summary>
    /// <param name="name">The name of the plugin instance.</param>
    /// <param name="delete">The plugin instance will be deleted if true.</param>
    internal void UpdateSil(string name, bool delete)
    {
        SilUpdate?.Invoke(name, delete);
    }

    /// <summary>
    /// Triggers the <see cref="TcRestart"/> event.
    /// </summary>
    internal void TriggerTcRestart()
    {
        TcRestart?.Invoke();
    }

    /// <summary>
    /// Sends a message to the remote connection of the <see cref="OC.Assistant"/>.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public async Task SendMessageAsync(string message)
    {
        if (_node is null) return;
        await _node.Send($"{MESSAGE_IDENTIFIER}{message}");
    }

    /// <summary>
    /// Is raised when a message from the remote connection has been received.
    /// </summary>
    public event Action<string>? MessageReceived;

    /// <summary>
    /// Is raised when a config has been received.
    /// </summary>
    public event Action<XElement>? ConfigReceived;
    
    /// <summary>
    /// Is raised when the TimeScaling value has been changed.
    /// </summary>
    public event Action<double>? TimeScalingChanged;

    /// <summary>
    /// Is raised when a project is connected and TwinCAT has been restarted.
    /// </summary>
    public event Action? TcRestart;

    /// <summary>
    /// Is raised when <see cref="UpdateSil"/> has been called.
    /// </summary>
    public event Action<string, bool>? SilUpdate;
    
    private void OnReceived(ApiMessage message)
    {
        var str = message.ToString();
        var type = str.Substring(0, 3);
        var msg = str.Remove(0, 3);

        switch (type)
        {
            case MESSAGE_IDENTIFIER:
                MessageReceived?.Invoke(msg);
                break;
            case CONFIG_IDENTIFIER:
                ConfigReceived?.Invoke(XElement.Load(msg));
                break;
            case TIMESCALE_IDENTIFIER:
                TimeScalingChanged?.Invoke(double.TryParse(msg, out var value) ? value : 1.0);
                break;
        }
    }
}