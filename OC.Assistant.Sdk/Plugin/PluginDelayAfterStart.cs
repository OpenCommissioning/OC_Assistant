namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Attribute class to define the milliseconds delay between the start of multiple plugins.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginDelayAfterStart : Attribute
{
    internal readonly int Value;
    
    /// <summary>
    /// Can be set to a specific milliseconds value.<br/>
    /// Is used to wait between the start of multiple plugins.
    /// <param name="value">Delay in milliseconds.</param>
    /// </summary>
    public PluginDelayAfterStart(int value)
    {
        Value = value;
    }
}