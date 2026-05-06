namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Attribute class to define which type of variables are generated.
/// </summary>
/// <remarks>
/// Obsolete. This attribute is not used anymore from SDK versions >= 1.8.0
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[Obsolete("This attribute is not used anymore from SDK versions >= 1.8.0")]
public class PluginIoType : Attribute
{
    internal readonly IoType Value;
    
    /// <summary>
    /// Defines which type of variables are generated.
    /// <param name="value"><see cref="IoType"/></param>
    /// </summary>
    public PluginIoType(IoType value)
    {
        Value = value;
    }
}