namespace OC.Assistant.Core;

/// <summary>
/// Interface to get informed about the project connection.
/// </summary>
public interface IConnectionState
{
    /// <summary>
    /// Is called when a TwinCAT project has been connected.
    /// </summary>
    /// <param name="solutionFullName">The full path of the solution file.</param>
    void OnConnect(string solutionFullName);
        
    /// <summary>
    /// Is called when the project has been disconnected.
    /// </summary>
    void OnDisconnect();
}