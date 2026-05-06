namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Attribute class to disable the cyclic read and write command.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginCustomReadWrite : Attribute
{
    /// <summary>
    /// Disables the cyclic read and write command.<br/>
    /// Can be used when a custom read and write command is needed.
    /// </summary>
    public PluginCustomReadWrite()
    {
    }
}