namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Attribute class to define a private field as a parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PluginParameter : Attribute
{
    internal readonly string? ToolTip;
    internal readonly string? FileFilter;
    
    /// <summary>
    /// Defines a private field as a parameter.
    /// </summary>
    /// <param name="toolTip">[optional] ToolTip when the mouse cursor is over the parameter.</param>
    /// <param name="fileFilter">[optional] Adds a Button for a
    /// OpenFileDialog with the given filter when not null.<br/></param>
    public PluginParameter(string? toolTip = null, string? fileFilter = null)
    {
        ToolTip = toolTip;
        FileFilter = fileFilter;
    }
}