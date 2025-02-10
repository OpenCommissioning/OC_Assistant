using EnvDTE;

namespace OC.Assistant.Core;

/// <summary>
/// Interface to implement events when a solution has been selected or closed.
/// </summary>
public interface IProjectSelector
{
    /// <summary>
    /// Is raised when a solution has been selected. 
    /// </summary>
    public event Action<DTE>? DteSelected;
}