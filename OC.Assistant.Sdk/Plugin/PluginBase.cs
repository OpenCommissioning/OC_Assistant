using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// The base class for a plugin.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginIoType"/><br/>can be used to define
/// the <see cref="IoType"/> of this plugin class.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginDelayAfterStart"/><br/>can be used to define the delay between
/// the start of multiple plugins.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginCustomReadWrite"/><br/> can be used to disable the cyclic read and write command.<br/>
/// <br/>
/// <c>Attribute</c> <see cref="PluginParameter"/><br/>can be used to define a private field as a parameter.
/// All parameters are shown in the properties window in the UI.<br/>
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
/// <c>Field</c> <see cref="OutputBuffer"/><br/>
/// is used to transfer data from the plugin to TwinCAT.
/// The size is either defined by the <see cref="OutputStructure"/> or the <see cref="OutputAddress"/>
/// depending on the attribute <see cref="PluginIoType"/>.<br/>
/// <br/>
/// <c>Field</c> <see cref="InputBuffer"/><br/>
/// is used to transfer data from TwinCAT to the plugin.
/// The size is defined by the <see cref="InputStructure"/> or the <see cref="InputAddress"/>
/// depending on the attribute <see cref="PluginIoType"/>.<br/>
/// <br/>
/// <c>Interface</c> <see cref="InputStructure"/><br/>
/// <inheritdoc cref="InputStructure"/><br/>
/// <br/>
/// <c>Interface</c> <see cref="OutputStructure"/><br/>
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
    /// Buffer with TwinCAT read data (e.g. plugin inputs).
    /// Is used to transfer data from TwinCAT to the plugin.<br/>
    /// <br/>
    /// The size is either defined by the <see cref="InputStructure"/> or the <see cref="InputAddress"/>
    /// depending on the attribute <see cref="PluginIoType"/>.<br/>
    /// <br/>
    /// Is updated before every <see cref="OnUpdate"/> cycle.
    /// </summary>
    protected byte[] InputBuffer => _tcAdsClient?.ReadBuffer ?? [];
        
    /// <summary>
    /// Buffer with TwinCAT write data (e.g. plugin outputs).
    /// Is used to transfer data from the plugin to TwinCAT.<br/>
    /// <br/>
    /// The size is either defined by the <see cref="OutputStructure"/> or the <see cref="OutputAddress"/>
    /// depending on the attribute <see cref="PluginIoType"/>.<br/>
    /// <br/>
    /// Is written after every <see cref="OnUpdate"/> cycle.
    /// </summary>
    protected byte[] OutputBuffer => _tcAdsClient?.WriteBuffer ?? [];

    /// <summary>
    /// Writes data from the <see cref="OutputBuffer"/> to TwinCAT.<br/>
    /// Is already called every cycle if <see cref="PluginCustomReadWrite"/> is not used.
    /// </summary>
    protected void TcWrite()
    {
        _tcAdsClient?.Write();
    }

    /// <summary>
    /// Writes data from a custom source to TwinCAT.<br/>
    /// </summary>
    /// <param name="source">The source data.</param>
    /// <param name="sourceOffset">The source offset.</param>
    /// <param name="destinationOffset">The TwinCAT relative offset.</param>
    /// <param name="length">Data length.</param>
    protected void TcWrite(byte[] source, int sourceOffset, int destinationOffset, int length)
    {
        _tcAdsClient?.Write(source, sourceOffset, destinationOffset, length);
    }

    /// <summary>
    /// Reads data from TwinCAT and copies to the <see cref="InputBuffer"/>.<br/>
    /// Is already called every cycle if <see cref="PluginCustomReadWrite"/> is not used.
    /// </summary>
    protected void TcRead()
    {
        _tcAdsClient?.Read();
    }
    
    /// <summary>
    /// Reads in- and output data from TwinCAT and copies to the
    /// <see cref="InputBuffer"/> and <see cref="OutputBuffer"/>.
    /// </summary>
    protected void TcReadAll()
    {
        _tcAdsClient?.ReadAll();
    }
        
    /// <summary>
    /// The cancellation token to stop all running tasks.
    /// </summary>
    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;
    
    /// <summary>
    /// Can be used to request a cancellation to stop the plugin.<br/>
    /// </summary>
    protected void CancellationRequest()
    {
        _cancellationTokenSource.Cancel();
    }

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
    /// Can be cancelled with <see cref="CancellationRequest"/> in case of an error.<br/>
    /// Before every update, the <see cref="InputBuffer"/> is updated.<br/>
    /// After every update, the <see cref="OutputBuffer"/> is written.
    /// </summary>
    protected abstract void OnUpdate();
    
    /// <summary>
    /// Is called once when the plugin stops.<br/>
    /// Can be used to disconnect or dispose members of the specific plugin.
    /// </summary>
    protected abstract void OnStop();
    
    /// <inheritdoc/>
    public IIoStructure InputStructure { get; } = new IoStructure(nameof(IPluginController.InputStructure));
    
    /// <inheritdoc/>
    public IIoStructure OutputStructure { get; } = new IoStructure(nameof(IPluginController.OutputStructure));
    
    /// <inheritdoc/>
    public int[] InputAddress { get; private set; } = [];
    
    /// <inheritdoc/>
    public int[] OutputAddress { get; private set; } = [];
    
    private bool _readyToStart = true;
    private string? _name;
    private bool _isRunning;
    private bool _ioChanged;
    private IoType _ioType = IoType.None;
    private int _delayAfterStart;
    private TcAdsClient? _tcAdsClient;
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly ParameterCollection _parameters = new();
    private bool _customReadWrite;
        
    [PluginParameter("e.g. 0-1023 or 0,1,2 or a combination")]
    private readonly string _inputAddress = "0-1023";
        
    [PluginParameter("e.g. 0-1023 or 0,1,2 or a combination")]
    private readonly string _outputAddress = "0-1023";

    [PluginParameter("Automatic start and stop with TwinCAT")]
    private readonly bool _autoStart = true;
    
    private void CollectAttributes()
    {
        var type = GetType();
        
        if (type.GetCustomAttribute(typeof(PluginDelayAfterStart)) is PluginDelayAfterStart delay)
        {
            _delayAfterStart = delay.Value;
        }

        if (type.GetCustomAttribute(typeof(PluginIoType)) is PluginIoType ioType)
        {
            _ioType = ioType.Value;
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

        if (_ioType != IoType.Address) return;
        _parameters.Add(this, type.BaseType?.GetField(nameof(_inputAddress), BindingFlags.NonPublic | BindingFlags.Instance));
        _parameters.Add(this, type.BaseType?.GetField(nameof(_outputAddress), BindingFlags.NonPublic | BindingFlags.Instance));
    }
    
    private void Cycle()
    {
        _readyToStart = false;
        Starting?.Invoke();

        try
        {
            InputStructure.Clear();
            OutputStructure.Clear();
            
            if (OnSave() && OnStart())
            {
                switch (_ioType)
                {
                    case IoType.None: break;
                    case IoType.Struct:
                        _tcAdsClient = new TcAdsClient(851, OutputStructure.Length, InputStructure.Length);
                        _tcAdsClient.SetReadIndex($"GVL_{_name}.Inputs");
                        _tcAdsClient.SetWriteIndex($"GVL_{_name}.Outputs");
                        break;
                    case IoType.Address:
                        _tcAdsClient = new TcAdsClient(851, OutputAddress.Length, InputAddress.Length);
                        _tcAdsClient.SetReadIndex($"GVL_{_name}.I{InputAddress[0]}");
                        _tcAdsClient.SetWriteIndex($"GVL_{_name}.Q{OutputAddress[0]}");
                        break;
                }
                
                _isRunning = true;
                Started?.Invoke();
                var stopwatch = new Stopwatch();
                while (!CancellationToken.IsCancellationRequested)
                {
                    stopwatch.WaitUntil(1);
                    if (_ioType != IoType.None && !_customReadWrite) _tcAdsClient?.Read();
                    OnUpdate();
                    if (_ioType != IoType.None && !_customReadWrite) _tcAdsClient?.Write();
                }

                Stopping?.Invoke();
                OnStop();
            }
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
        
        _isRunning = false;
        _readyToStart = true;
        _tcAdsClient?.Disconnect();
        Stopped?.Invoke();
    }

    IParameterCollection IPluginController.Parameter => _parameters;
    IoType IPluginController.IoType => _ioType;
    int IPluginController.DelayAfterStart => _delayAfterStart;
    bool IPluginController.IsRunning => _isRunning;
    bool IPluginController.AutoStart => _autoStart;
    bool IPluginController.IoChanged => _ioChanged;

    void IPluginController.Initialize(string? name)
    {
        _name = name;
        if (_ioType != IoType.Address) return;
        InputAddress = _inputAddress.ToNumberList();
        OutputAddress = _outputAddress.ToNumberList();
    }
    
    bool IPluginController.Save(string? name)
    {
        _name = name;
        _ioChanged = false;

        switch (_ioType)
        {
            case IoType.None:
                return OnSave();

            case IoType.Address:
                var inputAddress = _inputAddress.ToNumberList();
                var outputAddress = _outputAddress.ToNumberList();
                _ioChanged = !InputAddress.SequenceEqual(inputAddress);
                _ioChanged |= !OutputAddress.SequenceEqual(outputAddress);
                InputAddress = inputAddress;
                OutputAddress = outputAddress;
                return OnSave();
            
            case IoType.Struct:
                var inputStructure = new XElement(InputStructure.XElement);
                var outputStructure = new XElement(OutputStructure.XElement);
                InputStructure.Clear();
                OutputStructure.Clear();
                if (!OnSave()) return false;
                _ioChanged = !XNode.DeepEquals(inputStructure, InputStructure.XElement);
                _ioChanged |= !XNode.DeepEquals(outputStructure, OutputStructure.XElement);
                return true;

            default:
                return true;
        }
    }
    
    void IPluginController.Start()
    {
        if (!_readyToStart) return;
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(Cycle, CancellationToken);
    }
    
    void IPluginController.Stop()
    {
        _cancellationTokenSource.Cancel();
    }
    
    private event Action? Started;
    private event Action? Stopped;
    private event Action? Starting;
    private event Action? Stopping;
    
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
}