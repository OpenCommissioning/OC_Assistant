namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Attribute class to define which type of variables are generated in TwinCAT.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginIoType : Attribute
{
    internal readonly IoType Value;
    
    /// <summary>
    /// Defines which type of variables are generated in TwinCAT.
    /// <param name="value"><see cref="IoType"/></param>
    /// </summary>
    public PluginIoType(IoType value)
    {
        Value = value;
    }
}