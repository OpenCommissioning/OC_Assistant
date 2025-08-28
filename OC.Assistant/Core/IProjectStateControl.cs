namespace OC.Assistant.Core;

public interface IProjectStateControl
{
    /// <summary>
    /// Connects to a Visual Studio Solution via <see cref="EnvDTE.DTE"/> interface.
    /// </summary>
    /// <param name="solutionFullName">The path of the Visual Studio Solution.</param>
    /// <param name="projectFolder">The path of the project folder.</param>
    public void Connect(string solutionFullName, string projectFolder);
    
    /// <summary>
    /// Disconnects from the currently connected Visual Studio Solution.
    /// </summary>
    public void Disconnect();
    
    /// <summary>
    /// Triggers the application start state.
    /// </summary>
    public void Start();
    
    /// <summary>
    /// Triggers the application stop state.
    /// </summary>
    public void Stop();
}