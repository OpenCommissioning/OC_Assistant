namespace OC.Assistant.Core;

public interface IProjectStateControl
{
    /// <summary>
    /// Connects to a project file or a Visual Studio Solution.
    /// </summary>
    /// <param name="projectFile">The path of the project file or the Visual Studio Solution.</param>
    /// <param name="projectFolder">The path of the project folder, if any.</param>
    public void Connect(string projectFile, string? projectFolder = null);
    
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