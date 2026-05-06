using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents a wrapper around a <see cref="XElement"/> for plugin-specific content.
/// </summary>
public class XPlugin
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
    public XPlugin(IPlugin plugin)
    {
        if (plugin.Type is null || plugin.PluginController is null) return;
        
        Element = new XElement(nameof(Plugin),
            new XAttribute(nameof(Name), plugin.Name),
            new XAttribute(nameof(Type), plugin.Type.Name),
            new XAttribute(nameof(ChannelType), plugin.ChannelType?.Name ?? "<unknown>"),
            new XAttribute(nameof(InputSize), plugin.PluginController.InputStructure.Length),
            new XAttribute(nameof(OutputSize), plugin.PluginController.OutputStructure.Length),
            new XElement(nameof(Parameter), plugin.PluginController.Parameter.ToXElement().Nodes()),
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
    /// Gets the ChannelType attribute value.
    /// </summary>
    public string ChannelType => Element.GetOrCreateAttribute(nameof(ChannelType)).Value;
    
    /// <summary>
    /// Gets the InputSize in bytes.
    /// </summary>
    public int InputSize => 
        int.TryParse(Element.GetOrCreateAttribute(nameof(InputSize)).Value , out var value) ? value : 0;
    
    /// <summary>
    /// Gets the OutputSize in bytes.
    /// </summary>
    public int OutputSize => 
        int.TryParse(Element.GetOrCreateAttribute(nameof(OutputSize)).Value , out var value) ? value : 0;

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
}