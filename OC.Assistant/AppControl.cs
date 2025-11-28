using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using OC.Assistant.Controls;
using OC.Assistant.Sdk;

namespace OC.Assistant;

/// <summary>
/// Singleton class to interact with the application.
/// </summary>
internal class AppControl : IAppControl
{
    private static readonly Lazy<AppControl> LazyInstance = new(() => new AppControl());
    private string? _projectFile;
    private Dispatcher Dispatcher { get; } = Application.Current.Dispatcher;
    
    private AppControl()
    {
    }
    
    public static AppControl Instance => LazyInstance.Value;
    public double TimeScaling { get; private set; } = 1;
    public AppSettings Settings { get; } = new AppSettings().Read();
    public event Action<Type?>? PluginStartRequested;
    public event Action<Type?>? PluginStopRequested;
    public bool IsConnected => _projectFile is not null;
    
    public event Action<string>? Connected;
    public event Action? Disconnected;
    public event Action<XElement>? ConfigReceived;
    public event Action<string?, string?>? PluginUpdated;
    public event Action<double>? TimeScalingUpdated;

    public void UpdateConfig(XElement config) => ConfigReceived?.Invoke(config);
    
    public void UpdatePlugin(string? name, string? oldName) => PluginUpdated?.Invoke(name, oldName);

    public void UpdateTimeScaling(double timeScaling)
    {
        TimeScaling = timeScaling;
        TimeScalingUpdated?.Invoke(timeScaling);
    }

    public void AddMenuContent(object content) => MainMenu.AddMenuContent(content);

    public void AddWelcomePageContent(object content) => WelcomePage.AddContent(content);
    
    public void Connect(string projectFile)
    {
        Dispatcher.Invoke(() =>
        {
            if (_projectFile is not null) Disconnect();
            _projectFile = projectFile;
            XmlFile.Instance.Path = projectFile;
            StopPlugins();
            Connected?.Invoke(projectFile);
            Logger.LogInfo(this, $"{_projectFile} connected");
        });
    }

    public void Disconnect()
    {
        if (_projectFile is null) return;
        Dispatcher.Invoke(() =>
        {
            Logger.LogWarning(this, $"{_projectFile} disconnected");
            _projectFile = null;
            Disconnected?.Invoke();
        });
    }

    public void StartPlugins(Type? clientType= null)
    {
        if (_projectFile is null) return;
        Dispatcher.Invoke(() =>
        {
            PluginStartRequested?.Invoke(clientType);
        });
    }
    
    public void StopPlugins(Type? clientType = null)
    {
        if (_projectFile is null) return;
        Dispatcher.Invoke(() =>
        {
            PluginStopRequested?.Invoke(clientType);
        });
    }
}