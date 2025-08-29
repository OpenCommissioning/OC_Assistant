using System.Reflection;
using System.Windows;
using OC.Assistant.Sdk;

namespace OC.Assistant.Core;

/// <summary>
/// Singleton class for the project state.
/// </summary>
public class ProjectState : IProjectStateEvents, IProjectStateControl
{
    private static readonly Lazy<ProjectState> LazyInstance = new(() => new ProjectState());
    private string? _projectFile;
    
    /// <summary>
    /// Gets the <see cref="IProjectStateEvents"/> interface.
    /// </summary>
    public static IProjectStateEvents Events => LazyInstance.Value;
    
    /// <summary>
    /// Gets the <see cref="IProjectStateControl"/> interface.
    /// </summary>
    public static IProjectStateControl Control => LazyInstance.Value;
    
    /// <summary>
    /// Gets a value indicating whether the connected project is currently running.
    /// </summary>
    public static bool IsRunning { get; private set; }
    
    public event Action<string, string?>? Connected;
    public event Action? Disconnected;
    public event Action? StartedRunning;
    public event Action? StoppedRunning;
    public event Action<bool>? Locked;

    private ProjectState()
    {
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
                Connected?.Invoke(projectFile, null);
                Logger.LogInfo(this, $"{_projectFile} connected");
                Stop();
                return;
            }
            
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            XmlFile.Instance.Path = System.IO.Path.Combine(projectFolder, $"{assemblyName}.xml");
            Connected?.Invoke(projectFile, projectFolder);
            Logger.LogInfo(this, $"{_projectFile} connected");
        });
    }
    
    public void Disconnect()
    {
        if (_projectFile is null) return;
        Application.Current.Dispatcher.Invoke(() =>
        {
            Logger.LogWarning(this, $"{_projectFile} disconnected");
            IsRunning = false;
            _projectFile = null;
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
}