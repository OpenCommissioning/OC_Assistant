namespace OC.Assistant.Core;

/// <summary>
/// Interface to get informed about the project connection.
/// </summary>
public interface IConnectionState
{
    /// <summary>
    /// Is called when a project has been connected.
    /// </summary>
    void OnConnect();
        
    /// <summary>
    /// Is called when the project has been disconnected.
    /// </summary>
    void OnDisconnect();
}