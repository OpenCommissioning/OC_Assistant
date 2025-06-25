using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Core;

/// <summary>
/// Represents a wrapper around a <see cref="XElement"/> for plugin-specific properties.
/// </summary>
public class XPlugin
{
    /// <summary>
    /// The node name for the plugin.
    /// </summary>
    private const string PLUGIN = "Plugin";
    
    /// <summary>
    /// The name of the name attribute.
    /// </summary>
    private const string NAME = "Name";
    
    /// <summary>
    /// The name of the type attribute.
    /// </summary>
    private const string TYPE = "Type";
    
    /// <summary>
    /// The name of the ioType attribute.
    /// </summary>
    private const string IO_TYPE = nameof(OC.Assistant.Sdk.Plugin.IoType);
    
    /// <summary>
    /// The node name for parameters.
    /// </summary>
    private const string PARAMETER = nameof(IPluginController.Parameter);
    
    /// <summary>
    /// The node name for the input structure.
    /// </summary>
    private const string INPUT_STRUCT = nameof(IPluginController.InputStructure);
    
    /// <summary>
    /// The node name for the output structure.
    /// </summary>
    private const string OUTPUT_STRUCT = nameof(IPluginController.OutputStructure);

    /// <summary>
    /// The name of the predefined <see cref="PARAMETER"/> for the input address
    /// when <see cref="IO_TYPE"/> is set to <see cref="IoType.Address"/>.
    /// </summary>
    private const string INPUT_ADDRESS = nameof(IPluginController.InputAddress);
    
    /// <summary>
    /// The name of the predefined <see cref="PARAMETER"/> for the output address
    /// when <see cref="IO_TYPE"/> is set to <see cref="IoType.Address"/>.
    /// </summary>
    private const string OUTPUT_ADDRESS = nameof(IPluginController.OutputAddress);
    
    /// <summary>
    /// The underlying <see cref="XElement"/>.
    /// </summary>
    public XElement Element { get; }
    
    /// <summary>
    /// Creates a <see cref="XPlugin"/> based on an existing <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The given <see cref="XElement"/></param>
    public XPlugin(XElement element)
    {
        Element = element;
    }
    
    /// <summary>
    /// Creates a <see cref="XPlugin"/> with a new <see cref="XElement"/>.
    /// </summary>
    /// <param name="name">The name of the plugin.</param>
    /// <param name="type">The <see cref="System.Type"/> of the plugin.</param>
    /// <param name="ioType">The <see cref="IoType"/> of the plugin.</param>
    public XPlugin(string name, Type type, IoType ioType)
    {
        Element = new XElement(PLUGIN,
            new XAttribute(NAME, name),
            new XAttribute(TYPE, type.Name),
            new XAttribute(IO_TYPE, ioType));
    }
    
    public string Name => Element.GetOrCreateAttribute(NAME).Value;
    public string Type => Element.GetOrCreateAttribute(TYPE).Value;
    public string IoType => Element.GetOrCreateAttribute(IO_TYPE).Value;
    public XElement Parameter => Element.GetOrCreateChild(PARAMETER);
    public XElement? InputStructure => Element.Element(INPUT_STRUCT);
    public XElement? OutputStructure => Element.Element(OUTPUT_STRUCT);
    public int[]? InputAddress => Parameter.Element(INPUT_ADDRESS)?.Value.ToNumberList();
    public int[]? OutputAddress => Parameter.Element(OUTPUT_ADDRESS)?.Value.ToNumberList();
}