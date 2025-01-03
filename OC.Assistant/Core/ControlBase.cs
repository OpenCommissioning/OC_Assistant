using OC.Assistant.Core.TwinCat;
using TCatSysManagerLib;

namespace OC.Assistant.Core;

public abstract class ControlBase : System.Windows.Controls.UserControl, IProjectConnector
{
    protected ControlBase()
    {
        ProjectManager.Instance.Subscribe(this);
    }
    
    void IProjectConnector.Connect(TcDte tcDte)
    {
        TcDte = tcDte;
        TcProjectFolder = tcDte.GetProjectFolder();
        TcSysManager = tcDte.GetTcSysManager();
    }
    
    public abstract void OnConnect();
    
    public abstract void OnDisconnect();
    
    public abstract void OnTcStopped();
    
    public abstract void OnTcStarted();
    
    public virtual bool IsLocked
    {
        set => IsEnabled = !value;
    }
    
    public ITcSysManager15? TcSysManager { get; private set; }
    
    public string? TcProjectFolder { get; private set; }
    
    /// <summary>
    /// The connected <see cref="TcDte"/>.
    /// </summary>
    protected TcDte? TcDte { get; private set; }

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