namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Defines which type of variables are generated in TwinCAT.<br/>
/// </summary>
public enum IoType
{
    /// <summary>
    /// No TwinCAT variables.
    /// </summary>
    None,
    /// <summary>
    /// Address based variables in the TwinCAT plugin GVL.<br/>
    /// e.g. I100, I101, Q100, Q101.
    /// </summary>
    Address,
    /// <summary>
    /// Specific structures within the TwinCAT plugin GVL.<br/>
    /// There will be an Inputs  and an Outputs structure.<br/>
    /// The structures can be defined with the
    /// <see cref="PluginBase.InputStructure"/> and
    /// <see cref="PluginBase.OutputStructure"/> interfaces.
    /// </summary>
    Struct
}