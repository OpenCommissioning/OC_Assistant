using EnvDTE;

namespace OC.Assistant.Core;

/// <summary>
/// Interface for the <see cref="ProjectState"/> solution.
/// </summary>
public interface IProjectStateSolution
{
    /// <summary>
    /// Connects to a Visual Studio Solution via <see cref="DTE"/> interface.
    /// </summary>
    /// <param name="solutionFullName">The path of the Visual Studio Solution.</param>
    /// <param name="projectFolder">The path of the project folder.</param>
    public void Connect(string solutionFullName, string projectFolder);
    
    /// <summary>
    /// Gets the full name of the Visual Studio Solution.
    /// </summary>
    public string? FullName { get; }
}