namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents the interface for a plugin parameter. 
/// </summary>
public interface IParameter
{
    /// <summary>
    /// The parameter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The parameter value.
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// The parameter tooltip, if any.
    /// </summary>
    public object? ToolTip { get; }
    
    /// <summary>
    /// The file filter, if any.
    /// </summary>
    public string? FileFilter { get; }
}