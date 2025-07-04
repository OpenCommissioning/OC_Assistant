using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

/// <summary>
/// Represents a wrapper around a <see cref="XElement"/> for plugin-specific content.
/// </summary>
internal class XPlugin
{
    /// <summary>
    /// Gets the underlying <see cref="XElement"/>.
    /// </summary>
    public XElement? Element { get; }
    
    /// <summary>
    /// Creates a <see cref="XPlugin"/> based on a <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The given <see cref="XElement"/></param>
    public XPlugin(XElement element)
    {
        Element = element;
    }

    /// <summary>
    /// Creates a <see cref="XPlugin"/> based on a <see cref="Plugin"/>.
    /// </summary>
    /// <param name="plugin">The given <see cref="Plugin"/>.</param>
    public XPlugin(Plugin plugin)
    {
        if (!plugin.IsValid) return;
        
        Element = new XElement(nameof(Plugin),
            new XAttribute(nameof(Name), plugin.Name),
            new XAttribute(nameof(Type), plugin.Type.Name),
            new XAttribute(nameof(IoType), plugin.PluginController.IoType.ToString()),
            new XElement(nameof(Parameter), plugin.PluginController.Parameter.XElement.Nodes()),
            new XElement(nameof(InputStructure), plugin.PluginController.InputStructure.XElement.Nodes()),
            new XElement(nameof(OutputStructure), plugin.PluginController.OutputStructure.XElement.Nodes()));
    }
    
    /// <summary>
    /// Gets the Name attribute value.
    /// </summary>
    public string Name => Element.GetOrCreateAttribute(nameof(Name)).Value;

    /// <summary>
    /// Gets the Type attribute value.
    /// </summary>
    public string Type => Element.GetOrCreateAttribute(nameof(Type)).Value;

    /// <summary>
    /// Gets the IoType attribute value.
    /// </summary>
    public string IoType => Element.GetOrCreateAttribute(nameof(IoType)).Value;

    /// <summary>
    /// Gets the Parameter <see cref="XElement"/>.
    /// </summary>
    public XElement Parameter => Element.GetOrCreateChild(nameof(Parameter));

    /// <summary>
    /// Gets the InputStructure <see cref="XElement"/>.
    /// </summary>
    public XElement InputStructure => Element.GetOrCreateChild(nameof(InputStructure));

    /// <summary>
    /// Gets the OutputStructure <see cref="XElement"/>.
    /// </summary>
    public XElement OutputStructure => Element.GetOrCreateChild(nameof(OutputStructure));

    /// <summary>
    /// Gets the InputAddress value, converted to a list of numbers, if any.
    /// </summary>
    public int[]? InputAddress => 
        Parameter.Element(nameof(Sdk.Plugin.IPluginController.InputAddress))?.Value.ToNumberList();
    
    /// <summary>
    /// Gets the OutputAddress value, converted to a list of numbers, if any.
    /// </summary>
    public int[]? OutputAddress => 
        Parameter.Element(nameof(Sdk.Plugin.IPluginController.OutputAddress))?.Value.ToNumberList();
}