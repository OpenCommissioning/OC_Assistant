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
    /// <param name="dte">The <see cref="DTE"/> interface of the Visual Studio Solution.</param>
    public void Connect(DTE dte);
    
    /// <summary>
    /// Gets the full name of the Visual Studio Solution.
    /// </summary>
    public string? FullName { get; }
}