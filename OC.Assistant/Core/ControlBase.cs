using EnvDTE;
using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Core;

public abstract class ControlBase : System.Windows.Controls.UserControl, IProjectConnector
{
    protected ControlBase()
    {
        ProjectManager.Instance.Subscribe(this);
    }
    
    void IProjectConnector.Connect(DTE tcDte)
    {
        TcProjectFolder = tcDte.GetProjectFolder();
        SolutionFullName = tcDte.GetSolutionFullName();
    }
    
    public abstract void OnConnect();
    
    public abstract void OnDisconnect();
    
    public abstract void OnTcStopped();
    
    public abstract void OnTcStarted();
    
    public virtual bool IsLocked
    {
        set => IsEnabled = !value;
    }

    public string? SolutionFullName { get; private set; }

    public string? TcProjectFolder { get; private set; }

    /// <summary>
    /// Sets the <see cref="BusyState"/> for this control.
    /// <returns>True if the global <see cref="BusyState"/> is set, otherwise false.</returns>
    /// </summary>
    protected bool IsBusy
    {
        get => BusyState.IsSet;
        set
        {
            if (value)
            {
                BusyState.Set(this);
                return;
            }
            BusyState.Reset(this);
        }
    }
}