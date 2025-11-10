using System.Windows;
using System.Xml.Linq;
using OC.Assistant.Api;
using OC.Assistant.Controls;
using OC.Assistant.Plugins;
using OC.Assistant.Sdk;

namespace OC.Assistant;

/// <summary>
/// Singleton class to interact with the application.
/// </summary>
internal class AppControl : IAppControl
{
    private static readonly Lazy<AppControl> LazyInstance = new(() => new AppControl());
    private string? _projectFile;
    
    public static AppControl Instance => LazyInstance.Value;
    
    public IClient? PluginOnStart(int writeSize, int readSize)
    {
        return PluginStarted?.Invoke(writeSize, readSize);
    }
    
    public event Action<string, CommunicationType, object?>? Connected;
    public event Action? Disconnected;
    public event Action? StartedRunning;
    public event Action? StoppedRunning;
    public event Action<bool>? Locked;
    public event Action<XElement>? ConfigReceived;
    public event Action<string?, string?>? PluginUpdated;
    public event Func<int, int, IClient>? PluginStarted;
    public bool IsRunning { get; private set; }

    private AppControl()
    {
        if (LazyInstance.IsValueCreated) return;
        WebApi.ConfigReceived += ConfigReceived;
        PluginManager.PluginUpdated += PluginUpdated;
        
        // Deprecated named pipe API. Will be removed in future versions
        NamedPipeApi.Interface.ConfigReceived += ConfigReceived;
    }

    public void Connect(string projectFile, CommunicationType mode = CommunicationType.Default, object? parameter = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_projectFile is not null) Disconnect();
            _projectFile = projectFile;
            XmlFile.Instance.Path = projectFile;
            Connected?.Invoke(projectFile, mode, parameter);
            Logger.LogInfo(this, $"{_projectFile} connected");

            if (parameter is null) Instance.PluginStarted += OnPluginStarted;
        });
    }

    private static MemoryClient OnPluginStarted(int writeSize, int readSize) => new (writeSize, readSize);

    public void Disconnect()
    {
        if (_projectFile is null) return;
        Application.Current.Dispatcher.Invoke(() =>
        {
            Logger.LogWarning(this, $"{_projectFile} disconnected");
            IsRunning = false;
            _projectFile = null;
            Instance.PluginStarted -= OnPluginStarted;
            Disconnected?.Invoke();
            Locked?.Invoke(true);
        });
    }
    
    public void Start()
    {
        if (_projectFile is null) return;
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = true;
            StartedRunning?.Invoke();
            Locked?.Invoke(true);
        });
    }

    public void Stop()
    {
        if (_projectFile is null) return;
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            StoppedRunning?.Invoke();
            Locked?.Invoke(false);
        });
    }

    public void AddWelcomePageContent(object content)
    {
        WelcomePage.AddContent(content);
    }
}