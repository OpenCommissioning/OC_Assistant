using EnvDTE;
using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Core;

/// <summary>
/// Interface for the <see cref="ControlBase"/>.
/// </summary>
public interface IProjectConnector : IConnectionState
{
    /// <summary>
    /// Connects a TwinCAT project.
    /// </summary>
    /// <param name="tcDte">The <see cref="TcDte"/> to connect.</param>
    void Connect(DTE tcDte);
    
    /// <summary>
    /// Is called when TwinCAT started running.
    /// </summary>
    void OnTcStarted();
    
    /// <summary>
    /// Is called when TwinCAT stopped running.
    /// </summary>
    void OnTcStopped();
    
    /// <summary>
    /// Locks or unlocks the control.
    /// </summary>
    bool IsLocked { set; }
    
    /// <summary>
    /// The TwinCAT solution path.
    /// </summary>
    string? SolutionFullName { get; }
    
    /// <summary>
    /// The TwinCAT project folder path.
    /// </summary>
    string? TcProjectFolder { get; }
}