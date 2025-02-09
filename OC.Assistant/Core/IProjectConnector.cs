namespace OC.Assistant.Core;

/// <summary>
/// Interface for the <see cref="ControlBase"/>.
/// </summary>
public interface IProjectConnector : IConnectionState
{
    /// <summary>
    /// Connects a TwinCAT project.
    /// </summary>
    /// <param name="solutionFullName">The full path of the solution file.</param>
    void Connect(string solutionFullName);
    
    /// <summary>
    /// Disconnects the TwinCAT project.
    /// </summary>
    void Disconnect();
    
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
}