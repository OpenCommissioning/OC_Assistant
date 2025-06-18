using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

/// <summary>
/// <see cref="Core.XmlFile"/> extension to write and read plugin configurations.
/// </summary>
internal static class XmlFileExtension
{
    /// <summary>
    /// Removes the given plugin.
    /// </summary>
    public static void RemovePlugin(this XmlFile xmlFile, Plugin plugin)
    {
        foreach (var item in xmlFile.Plugins.Elements())
        {
            if (item.Attribute(XmlTags.PLUGIN_NAME)?.Value == plugin.Name) item.Remove();
        }
        
        xmlFile.Save();
    }
    
    /// <summary>
    /// Updates or adds the given plugin.
    /// </summary>
    public static void UpdatePlugin(this XmlFile xmlFile, Plugin plugin)
    {
        xmlFile.RemovePlugin(plugin);

        var xElement = new XElement(XmlTags.PLUGIN,
            new XAttribute(XmlTags.PLUGIN_NAME, plugin.Name ?? ""),
            new XAttribute(XmlTags.PLUGIN_TYPE, plugin.Type?.Name ?? ""),
            new XAttribute(XmlTags.PLUGIN_IO_TYPE, plugin.PluginController?.IoType ?? IoType.None),
            plugin.PluginController?.Parameter.XElement,
            plugin.PluginController?.InputStructure.XElement,
            plugin.PluginController?.OutputStructure.XElement);

        xmlFile.Plugins.Add(xElement);
        xmlFile.Save();
    }
        
    /// <summary>
    /// Loads all plugins.
    /// </summary>
    public static List<Plugin> LoadPlugins(this XmlFile xmlFile)
    {
        var plugins = new List<Plugin>();

        foreach (var element in xmlFile.Plugins.Elements())
        {
            var name = element.Attribute(XmlTags.PLUGIN_NAME)?.Value;
            var type = element.Attribute(XmlTags.PLUGIN_TYPE)?.Value;
            var parameter = element.Element(XmlTags.PLUGIN_PARAMETER);
            var pluginInfo = PluginRegister.GetByTypeName(type);
            if (pluginInfo is null)
            {
                Logger.LogWarning(typeof(XmlFile), 
                    $"Plugin for type '{type}' not found in directory {PluginRegister.SearchPath}");
                continue;
            }
            if (name is null || parameter is null) continue;
            plugins.Add(new Plugin(name, pluginInfo.Type, parameter));
        }
            
        return plugins;
    }
}