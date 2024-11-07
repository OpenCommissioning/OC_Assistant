using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Core;

/// <summary>
/// Predefined strings for the xml file.
/// </summary>
public struct XmlTags
{
    /// <summary>
    /// The name of the root node.
    /// </summary>
    public const string ROOT = "Config";

    /// <summary>
    /// The node name for the plugin category.
    /// </summary>
    public const string PLUGINS = "Plugins";
    
    /// <summary>
    /// The node name for the plc project category.
    /// </summary>
    public const string PROJECT = "Project";

    /// <summary>
    /// The node name for the general settings.
    /// </summary>
    public const string SETTINGS = "Settings";
    
    /// <summary>
    /// The node name for the projectName setting.
    /// </summary>
    public const string PLC_PROJECT_NAME = "PlcProjectName";
    
    /// <summary>
    /// The node name for the taskName setting.
    /// </summary>
    public const string PLC_TASK_NAME = "PlcTaskName";
    
    /// <summary>
    /// The node name for the TaskAutoUpdate setting.
    /// </summary>
    public const string TASK_AUTO_UPDATE = "TaskAutoUpdate";
    
    /// <summary>
    /// The node name for Hil in the <see cref="PROJECT"/> category.
    /// </summary>
    public const string HIL = "Hil";
    
    /// <summary>
    /// The node name for the plc project root in the <see cref="PROJECT"/> category.
    /// </summary>
    public const string MAIN = "Main";
    
    /// <summary>
    /// The node name for a plugin in the <see cref="PLUGINS"/> category.
    /// </summary>
    public const string PLUGIN = "Plugin";
    
    /// <summary>
    /// The node name for parameters in the <see cref="PLUGINS"/> category.
    /// </summary>
    public const string PLUGIN_PARAMETER = nameof(IPluginController.Parameter);
    
    /// <summary>
    /// The node name for the input structure in the <see cref="PLUGINS"/> category.
    /// </summary>
    public const string PLUGIN_INPUT_STRUCT = nameof(IPluginController.InputStructure);
    
    /// <summary>
    /// The node name for output structure in the <see cref="PLUGINS"/> category.
    /// </summary>
    public const string PLUGIN_OUTPUT_STRUCT = nameof(IPluginController.OutputStructure);
    
    /// <summary>
    /// The name of the name attribute for a <see cref="PLUGIN"/>.
    /// </summary>
    public const string PLUGIN_NAME = "Name";
    
    /// <summary>
    /// The name of the type attribute for a <see cref="PLUGIN"/>.
    /// </summary>
    public const string PLUGIN_TYPE = "Type";
    
    /// <summary>
    /// The name of the ioType attribute for a <see cref="PLUGIN"/>.
    /// </summary>
    public const string PLUGIN_IO_TYPE = "IoType";

    /// <summary>
    /// The name of the predefined <see cref="PLUGIN_PARAMETER"/> for the input address
    /// when <see cref="PLUGIN_IO_TYPE"/> is set to <see cref="IoType.Address"/>.
    /// </summary>
    public const string PLUGIN_PARAMETER_INPUT_ADDRESS = nameof(IPluginController.InputAddress);
    
    /// <summary>
    /// The name of the predefined <see cref="PLUGIN_PARAMETER"/> for the output address
    /// when <see cref="PLUGIN_IO_TYPE"/> is set to <see cref="IoType.Address"/>.
    /// </summary>
    public const string PLUGIN_PARAMETER_OUTPUT_ADDRESS = nameof(IPluginController.OutputAddress);
}