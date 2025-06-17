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
    /// Updates or removes the given plugin.
    /// </summary>
    public static void UpdatePlugin(this XmlFile xmlFile, Plugin plugin, bool remove = false)
    {
        var pluginElements = xmlFile.Plugins.Elements();

        foreach (var item in pluginElements)
        {
            if (item.Attribute(XmlTags.PLUGIN_NAME)?.Value == plugin.Name) item.Remove();
        }

        if (remove)
        {
            xmlFile.Save();
            return;
        }

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
        var pluginElements = xmlFile.Plugins.Elements().ToList();
        var plugins = new List<Plugin>();

        foreach (var element in pluginElements)
        {
            var name = element.Attribute(XmlTags.PLUGIN_NAME)?.Value;
            var type = element.Attribute(XmlTags.PLUGIN_TYPE)?.Value;
            var parameter = element.Element(XmlTags.PLUGIN_PARAMETER);
            var pluginType = PluginRegister.GetTypeByName(type);
            if (pluginType is null)
            {
                Logger.LogWarning(typeof(XmlFile), 
                    $"Plugin for type '{type}' not found in directory {PluginRegister.SearchPath}");
                continue;
            }
            if (name is null || parameter is null) continue;
            plugins.Add(new Plugin(name, pluginType, parameter));
        }
            
        return plugins;
    }
}