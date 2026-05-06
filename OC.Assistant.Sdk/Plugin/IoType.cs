namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Defines which type of variables are generated in TwinCAT.<br/>
/// </summary>
/// <remarks>
/// Obsolete. This enum is not used anymore from SDK versions >= 1.8.0
/// </remarks>
[Obsolete("This enum is not used anymore from SDK versions >= 1.8.0")]
public enum IoType
{
    /// <summary>
    /// No variables.
    /// </summary>
    None,
    /// <summary>
    /// Address-based variables.<br/>
    /// e.g. I100, I101, Q100, Q101.
    /// </summary>
    Address,
    /// <summary>
    /// There will be an Inputs  and an Outputs structure.<br/>
    /// The structures can be defined with the
    /// <see cref="PluginBase.InputStructure"/> and
    /// <see cref="PluginBase.OutputStructure"/> interfaces.
    /// </summary>
    Struct
}