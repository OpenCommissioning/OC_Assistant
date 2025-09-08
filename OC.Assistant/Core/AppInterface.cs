using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using OC.Assistant.Controls;
using OC.Assistant.Plugins;
using OC.Assistant.Sdk;

namespace OC.Assistant.Core;

/// <summary>
/// Singleton class to interact with the application.
/// </summary>
internal class AppInterface : IAppControl
{
    private static readonly Lazy<AppInterface> LazyInstance = new(() => new AppInterface());
    private string? _projectFile;
    
    public static AppInterface Instance => LazyInstance.Value;
    
    public IClient? PluginOnStart(int writeSize, int readSize)
    {
        return PluginStarted?.Invoke(writeSize, readSize);
    }
    
    public event Action<string, string?>? Connected;
    public event Action? Disconnected;
    public event Action? StartedRunning;
    public event Action? StoppedRunning;
    public event Action<bool>? Locked;
    public event Action<XElement>? ConfigReceived;
    public event Action<string?, string?>? PluginUpdated;
    public event Func<int, int, IClient>? PluginStarted;
    public bool IsRunning { get; private set; }

    private AppInterface()
    {
        if (LazyInstance.IsValueCreated) return;
        WebApi.ConfigReceived += ConfigReceived;
        PluginManager.PluginUpdated += PluginUpdated;
    }

    public void Connect(string projectFile, string? projectFolder = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_projectFile is not null) Disconnect();
            _projectFile = projectFile;

            if (projectFolder is null)
            {
                XmlFile.Instance.Path = projectFile;
                ApiLocal.Interface.CommunicationType = CommunicationType.TcpIp;
                Instance.PluginStarted += OnPluginStarted;
                Connected?.Invoke(projectFile, null);
                Logger.LogInfo(this, $"{_projectFile} connected");
                Stop();
                return;
            }
            
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            XmlFile.Instance.Path = System.IO.Path.Combine(projectFolder, $"{assemblyName}.xml");
            ApiLocal.Interface.CommunicationType = CommunicationType.Twincat;
            Connected?.Invoke(projectFile, projectFolder);
            Logger.LogInfo(this, $"{_projectFile} connected");
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