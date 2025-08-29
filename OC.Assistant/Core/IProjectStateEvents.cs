namespace OC.Assistant.Core;

/// <summary>
/// Interface for the <see cref="ProjectState"/> events.
/// </summary>
public interface IProjectStateEvents
{
    /// <summary>
    /// Is raised with the project file and folder when a project gets connected.
    /// </summary>
    public event Action<string, string?>? Connected;
    
    /// <summary>
    /// Is raised when the project gets disconnected.
    /// </summary>
    public event Action? Disconnected;
    
    /// <summary>
    /// Is raised when a project is connected and TwinCAT started running.
    /// </summary>
    public event Action? StartedRunning;
    
    /// <summary>
    /// Is raised when a project is connected and TwinCAT stopped running.
    /// </summary>
    public event Action? StoppedRunning;
    
    /// <summary>
    /// Is raised with <c>True</c> when the project gets disconnected or when TwinCAT started running.<br/>
    /// Is raised with <c>False</c> when a project is connected and TwinCAT stopped running.
    /// </summary>
    public event Action<bool>? Locked;
}