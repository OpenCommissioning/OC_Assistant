using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using EnvDTE;
using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Core;

/// <summary>
/// Manager to control inheritors of <see cref="IProjectConnector"/>,
/// <see cref="IProjectSelector"/> and
/// <see cref="IConnectionState"/>.
/// </summary>
public class ProjectManager
{
    private readonly List<object> _controls = [];
    private bool _hasBeenConnected;
    private readonly Grid _grid = new ();
    private static readonly Lazy<ProjectManager> LazyInstance = new(() => new ProjectManager());
    private bool _initialized;
    
    /// <summary>
    /// Creates a new instance of the <see cref="ProjectManager"/> once.
    /// </summary>
    private ProjectManager()
    {
        if (LazyInstance.IsValueCreated) return;
        
        var tcState = new TcState
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 6, 0)
        };
        tcState.StartedRunning += OnStartedRunning;
        tcState.StoppedRunning += OnStoppedRunning;
        
        _grid.Children.Add(tcState);
    }
    
    /// <summary>
    /// Static instance of the <see cref="ProjectManager"/>.
    /// </summary>
    public static ProjectManager Instance => LazyInstance.Value;

    /// <summary>
    /// Initializes the <see cref="ProjectManager"/> and adds it to an element. Can only be done once.
    /// </summary>
    /// <param name="parent">An inheritor of <see cref="IAddChild"/> to add the <see cref="ProjectManager"/> as child.</param>
    public void Initialize(IAddChild parent)
    {
        if (_initialized) return;
        _initialized = true;
        
        foreach (var control in _controls)
        {
            switch (control)
            {
                case IProjectConnector connector:
                    connector.IsLocked = true;
                    break;
                case IProjectSelector projectSelector:
                    projectSelector.DteSelected += Connect;
                    projectSelector.DteClosed += Disconnect;
                    break;
            }
        }
        
        parent.AddChild(_grid);
    }
    
    /// <summary>
    /// Adds the given control to the <see cref="ProjectManager"/> pool.
    /// </summary>
    /// <param name="control">Can implement
    /// <see cref="IProjectConnector"/>,
    /// <see cref="IProjectSelector"/> or
    /// <see cref="IConnectionState"/>.</param>
    public void Subscribe(object control)
    {
        if (control is IProjectConnector or IProjectSelector or IConnectionState)
        {
            _controls.Add(control);
        }
    }
        
    /// <summary>
    /// Connects all subscribed controls to the given project.
    /// </summary>
    /// <param name="selectedDte"><see cref="DTE"/> solution of the project.</param>
    private void Connect(DTE selectedDte)
    {
        _grid.Dispatcher.Invoke(() =>
        {
            var solutionFullName = selectedDte.GetSolutionFullName();
            if (solutionFullName is null) return;
            if (RestartIfConnected(solutionFullName)) return;
        
            XmlFile.Directory = selectedDte.GetProjectFolder();
            
            foreach (var control in _controls.OfType<IConnectionState>())
            {
                control.OnConnect(solutionFullName);
            }
            
            _hasBeenConnected = true;
        });
    }
    
    /// <summary>
    /// Disconnects all subscribed controls from the project.
    /// </summary>
    private void Disconnect()
    {
        _grid.Dispatcher.Invoke(() =>
        {
            Sdk.Logger.LogWarning(this, "TwinCAT Project closed");
            
            foreach (var control in _controls.OfType<IProjectConnector>())
            {
                control.IsLocked = true;
            }
            foreach (var control in _controls.OfType<IConnectionState>())
            {
                control.OnDisconnect();
            }
        });
    }
    
    private void OnStoppedRunning()
    {
        _grid.Dispatcher.Invoke(() =>
        {
            foreach (var control in _controls.OfType<IProjectConnector>())
            {
                control.IsLocked = false;
                control.OnTcStopped();
            }
        });
    }
        
    private void OnStartedRunning()
    {
        _grid.Dispatcher.Invoke(() =>
        {
            foreach (var control in _controls.OfType<IProjectConnector>())
            {
                control.IsLocked = true;
                control.OnTcStarted();
            }
        });
    }
    
    private bool RestartIfConnected(string projectPath)
    {
        if (!_hasBeenConnected)
        {
            return false;
        }
        
        System.IO.File.WriteAllText(AppData.PreselectedProject, projectPath);

        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(processPath))
        {
            System.Diagnostics.Process.Start(processPath);
        }
        Application.Current.Shutdown();
        return true;
    }
}