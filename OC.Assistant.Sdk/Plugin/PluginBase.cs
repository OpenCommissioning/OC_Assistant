using System.Reflection;
using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// The base class for a plugin.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginDelayAfterStart"/><br/>can be used to define the delay between
/// the start of multiple plugins.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginCustomReadWrite"/><br/> can be used to disable the cyclic read and write command.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginParameter"/><br/>can be used to define a private field as a parameter.
/// All parameters are shown in the property window in the UI.<br/>
/// <br/>
/// <c>Method</c> <see cref="OnSave"/><br/>
/// <inheritdoc cref="OnSave"/><br/>
/// Should return true if successful, otherwise false.<br/>
/// <br/>
/// <c>Method</c> <see cref="OnStart"/><br/>
/// <inheritdoc cref="OnStart"/><br/>
/// Should return true if successful, otherwise false.<br/>
/// <br/>
/// <c>Method</c> <see cref="OnUpdate"/><br/>
/// <inheritdoc cref="OnUpdate"/><br/>
/// <br/>
/// <c>Method</c> <see cref="OnStop"/><br/>
/// <inheritdoc cref="OnStop"/><br/>
/// <br/>
/// <c>Property</c> <see cref="OutputBuffer"/><br/>
/// is used to transfer data from the plugin to the server.
/// The size is defined by the <see cref="OutputStructure"/>.<br/>
/// <br/>
/// <c>Property</c> <see cref="InputBuffer"/><br/>
/// is used to transfer data from the server to the plugin.
/// The size is defined by the <see cref="InputStructure"/>.<br/>
/// <br/>
/// <c>Property</c> <see cref="InputStructure"/><br/>
/// <inheritdoc cref="InputStructure"/><br/>
/// <br/>
/// <c>Property</c> <see cref="OutputStructure"/><br/>
/// <inheritdoc cref="OutputStructure"/><br/>
/// </summary>
public abstract class PluginBase : IPluginController
{
    /// <summary>
    /// The base constructor. Creates a new instance of the <see cref="PluginBase"/>.
    /// </summary>
    protected PluginBase()
    {
        CollectAttributes();
        CollectParameters();
    }
    
    /// <summary>
    /// Buffer for plugin inputs.
    /// Is used to transfer data from the server to the plugin.<br/>
    /// <br/>
    /// The size is defined by the <see cref="InputStructure"/>.<br/>
    /// <br/>
    /// Is updated before every <see cref="OnUpdate"/> cycle.
    /// </summary>
    protected byte[] InputBuffer => _channel?.ReadBuffer ?? [];
        
    /// <summary>
    /// Buffer for plugin outputs.
    /// Is used to transfer data from the plugin to the server.<br/>
    /// <br/>
    /// The size is defined by the <see cref="OutputStructure"/>.<br/>
    /// <br/>
    /// Is written after every <see cref="OnUpdate"/> cycle.
    /// </summary>
    protected byte[] OutputBuffer => _channel?.WriteBuffer ?? [];
    
    /// <summary>
    /// The <see cref="Type"/> of the channel.
    /// </summary>
    protected Type? ChannelType => _channel?.GetType();

    /// <inheritdoc cref="ChannelBase.RecordDataServer"/>
    protected IRecordDataServer RecordDataServer => 
        _channel?.RecordDataServer ?? throw new InvalidOperationException("Not yet initialized.");

    /// <summary>
    /// Writes data from the <see cref="OutputBuffer"/> to the server.<br/>
    /// Is already called every cycle if <see cref="PluginCustomReadWrite"/> is not used.
    /// </summary>
    protected void TcWrite() => _channel?.Write();

    /// <summary>
    /// Writes data from a custom source to the server.<br/>
    /// </summary>
    /// <param name="source">The source data.</param>
    /// <param name="sourceOffset">The source offset.</param>
    /// <param name="destinationOffset">The server relative offset.</param>
    /// <param name="length">Data length.</param>
    protected void TcWrite(byte[] source, int sourceOffset, int destinationOffset, int length)
        => _channel?.Write(source, sourceOffset, destinationOffset, length);

    /// <summary>
    /// Reads data from the server and copies to the <see cref="InputBuffer"/>.<br/>
    /// Is already called every cycle if <see cref="PluginCustomReadWrite"/> is not used.
    /// </summary>
    protected void TcRead() => _channel?.Read();
    
    /// <summary>
    /// Reads in- and output data from the server and copies to the
    /// <see cref="InputBuffer"/> and <see cref="OutputBuffer"/>.
    /// </summary>
    protected void TcReadAll() => _channel?.ReadAll();
    
    /// <summary>
    /// Can be used to request a cancellation to stop the plugin.<br/>
    /// </summary>
    protected void CancellationRequest() => _cancellationTokenSource.Cancel();
    
    /// <summary>
    /// The cancellation token.
    /// </summary>
    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Is called when the plugin gets saved and before every start.
    /// </summary>
    /// <returns>True if successful, otherwise False.</returns>
    protected abstract bool OnSave();
    
    /// <summary>
    /// Is called once when the plugin starts.
    /// </summary>
    /// <returns>True if successful, otherwise False.</returns>
    protected abstract bool OnStart();
    
    /// <summary>
    /// Is called cyclic when <see cref="OnSave"/> and <see cref="OnStart"/> don't fail.<br/>
    /// Can be canceled with <see cref="CancellationRequest"/> in case of an error.<br/>
    /// Before every update, the <see cref="InputBuffer"/> is updated.<br/>
    /// After every update, the <see cref="OutputBuffer"/> is written.
    /// </summary>
    protected abstract void OnUpdate();
    
    /// <summary>
    /// Is called once when the plugin stops.<br/>
    /// Can be used to disconnect or dispose of members.
    /// </summary>
    protected abstract void OnStop();
    
    /// <summary>
    /// Variables should be added in the <see cref="PluginBase.OnSave"/> method.
    /// </summary>
    public IIoStructure InputStructure { get; } = new IoStructure(nameof(IPluginController.InputStructure));
    
    /// <summary>
    /// Variables should be added in the <see cref="PluginBase.OnSave"/> method.
    /// </summary>
    public IIoStructure OutputStructure { get; } = new IoStructure(nameof(IPluginController.OutputStructure));
    
    /// <summary>
    /// The instance name.
    /// </summary>
    protected string? Name { get; private set; }
    
    private bool _readyToStart = true;
    private bool _isRunning;
    private bool _ioChanged;
    private int _delayAfterStart;
    private ChannelBase? _channel;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly ParameterCollection _parameters = new();
    private bool _customReadWrite;

    [PluginParameter("Automatic start and stop with the application")]
    private readonly bool _autoStart = true;
    
    private void CollectAttributes()
    {
        var type = GetType();
        
        if (type.GetCustomAttribute(typeof(PluginDelayAfterStart)) is PluginDelayAfterStart delay)
        {
            _delayAfterStart = delay.Value;
        }
        
        if (type.GetCustomAttribute(typeof(PluginCustomReadWrite)) is PluginCustomReadWrite)
        {
            _customReadWrite = true;
        }
    }

    private void CollectParameters()
    {
        var type = GetType();
            
        _parameters.Add(this, type.BaseType?.GetField(nameof(_autoStart), BindingFlags.NonPublic | BindingFlags.Instance));
            
        foreach(var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            _parameters.Add(this, field);
        }
    }

    private void InitializeClient()
    {
        if (Name is null || _channel is null) return;
        
        _channel.SetReadIndex(Name, "Inputs");
        _channel.SetWriteIndex(Name, "Outputs");
    }
    
    private void Cycle()
    {
        _readyToStart = false;
        Starting?.Invoke();

        try
        {
            InputStructure.Clear();
            OutputStructure.Clear();
            
            if (OnSave())
            {
                _channel = ChannelRequested?.Invoke();
                if (_channel is not null && OnStart())
                {
                    _isRunning = true;
                    Started?.Invoke();
                    InitializeClient();
                    var stopwatch = new StopwatchEx();
                    var token = _cancellationTokenSource.Token;
                    while (!token.IsCancellationRequested)
                    {
                        stopwatch.WaitUntil(1);
                        if (!_customReadWrite) _channel?.Read();
                        OnUpdate();
                        if (!_customReadWrite) _channel?.Write();
                    }

                    Stopping?.Invoke();
                    OnStop();
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
        
        _isRunning = false;
        _readyToStart = true;
        _channel?.Disconnect();
        Stopped?.Invoke();
    }

    IParameterCollection IPluginController.Parameter => _parameters;
    int IPluginController.DelayAfterStart => _delayAfterStart;
    bool IPluginController.IsRunning => _isRunning;
    bool IPluginController.AutoStart => _autoStart;
    bool IPluginController.IoChanged => _ioChanged;

    void IPluginController.Initialize(string? name)
    {
        Name = name;
    }
    
    bool IPluginController.Save(string? name)
    {
        Name = name;
        _ioChanged = false;
        
        var inputStructure = new XElement(InputStructure.XElement);
        var outputStructure = new XElement(OutputStructure.XElement);
        InputStructure.Clear();
        OutputStructure.Clear();
        if (!OnSave()) return false;
        _ioChanged = !XNode.DeepEquals(inputStructure, InputStructure.XElement);
        _ioChanged |= !XNode.DeepEquals(outputStructure, OutputStructure.XElement);
        return true;
    }
    
    void IPluginController.Start()
    {
        if (_isRunning || !_readyToStart) return;
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(Cycle, _cancellationTokenSource.Token);
    }
    
    void IPluginController.Stop()
    {
        if (!_isRunning) return;
        _cancellationTokenSource.Cancel();
    }
    
    private event Action? Started;
    private event Action? Stopped;
    private event Action? Starting;
    private event Action? Stopping;
    private event Func<ChannelBase?>? ChannelRequested;
    
    event Action? IPluginController.Started
    {
        add => Started += value;
        remove => Started -= value;
    }
    
    event Action? IPluginController.Stopped
    {
        add => Stopped += value;
        remove => Stopped -= value;
    }
    
    event Action? IPluginController.Starting
    {
        add => Starting += value;
        remove => Starting -= value;
    }
    
    event Action? IPluginController.Stopping
    {
        add => Stopping += value;
        remove => Stopping -= value;
    }
    
    event Func<ChannelBase?>? IPluginController.ChannelRequested
    {
        add => ChannelRequested += value;
        remove => ChannelRequested -= value;
    }
}