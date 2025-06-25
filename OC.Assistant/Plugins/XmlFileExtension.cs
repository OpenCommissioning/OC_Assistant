using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

/// <summary>
/// <see cref="Core.XmlFile"/> extension to update and load plugins.
/// </summary>
internal static class XmlFileExtension
{
    /// <summary>
    /// Updates or adds the given <see cref="Plugin"/> to the <see cref="XmlFile"/>.
    /// </summary>
    public static void UpdatePlugin(this XmlFile xmlFile, Plugin plugin)
    {
        if (plugin.Type is null || plugin.PluginController is null) return;
        
        xmlFile.RemovePlugin(plugin.Name);
        
        var xPlugin = new XPlugin(plugin.Name, plugin.Type, plugin.PluginController.IoType);
        xPlugin.Element.Add(plugin.PluginController.Parameter.XElement);
        xPlugin.Element.Add(plugin.PluginController.InputStructure.XElement);
        xPlugin.Element.Add(plugin.PluginController.OutputStructure.XElement);
        
        xmlFile.Plugins.Add(xPlugin);
        xmlFile.Save();
    }
        
    /// <summary>
    /// Gets a list of plugins from the <see cref="XmlFile"/>.
    /// <returns>A <see cref="List{T}"/> of type <see cref="Plugin"/>.</returns>
    /// </summary>
    public static List<Plugin> LoadPlugins(this XmlFile xmlFile)
    {
        var plugins = new List<Plugin>();

        foreach (var xPlugin in xmlFile.PluginElements)
        {
            var pluginInfo = PluginRegister.GetByTypeName(xPlugin.Type);
            if (pluginInfo is null)
            {
                Logger.LogWarning(typeof(XmlFile), 
                    $"Plugin for type '{xPlugin.Type}' not found in directory {PluginRegister.SearchPath}");
                continue;
            }
            plugins.Add(new Plugin(xPlugin.Name, pluginInfo.Type, xPlugin.Parameter));
        }
            
        return plugins;
    }
}