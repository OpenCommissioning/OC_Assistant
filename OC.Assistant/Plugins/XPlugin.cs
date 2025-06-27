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
    public XElement Element { get; }
    
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
        Element = new XElement(nameof(Plugin));
        
        if (plugin.Type is null || plugin.PluginController is null) return;
        Name = plugin.Name;
        Type = plugin.Type.Name;
        IoType = plugin.PluginController.IoType.ToString();
        Parameter = plugin.PluginController.Parameter.XElement;
        InputStructure = plugin.PluginController.InputStructure.XElement;
        OutputStructure = plugin.PluginController.OutputStructure.XElement;
    }
    
    /// <summary>
    /// Gets the Name attribute value.
    /// </summary>
    public string Name
    {
        get => Element.GetOrCreateAttribute(nameof(Name)).Value;
        private init => Element.GetOrCreateAttribute(nameof(Name)).Value = value;       
    }

    /// <summary>
    /// Gets the Type attribute value.
    /// </summary>
    public string Type
    {
        get => Element.GetOrCreateAttribute(nameof(Type)).Value;
        private init => Element.GetOrCreateAttribute(nameof(Type)).Value = value;       
    }
    
    /// <summary>
    /// Gets the IoType attribute value.
    /// </summary>
    public string IoType
    {
        get => Element.GetOrCreateAttribute(nameof(IoType)).Value;
        private init => Element.GetOrCreateAttribute(nameof(IoType)).Value = value;       
    }
    
    /// <summary>
    /// Gets the Parameter <see cref="XElement"/>.
    /// </summary>
    public XElement Parameter
    {
        get => Element.GetOrCreateChild(nameof(Parameter));
        private init => Element.GetOrCreateChild(nameof(Parameter)).ReplaceNodes(value.Nodes());
    }
    
    /// <summary>
    /// Gets the InputStructure <see cref="XElement"/>.
    /// </summary>
    public XElement InputStructure
    {
        get => Element.GetOrCreateChild(nameof(InputStructure));
        private init => Element.GetOrCreateChild(nameof(InputStructure)).ReplaceNodes(value.Nodes());
    }
    
    /// <summary>
    /// Gets the OutputStructure <see cref="XElement"/>.
    /// </summary>
    public XElement OutputStructure
    {
        get => Element.GetOrCreateChild(nameof(OutputStructure));
        private init => Element.GetOrCreateChild(nameof(OutputStructure)).ReplaceNodes(value.Nodes());
    }
    
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