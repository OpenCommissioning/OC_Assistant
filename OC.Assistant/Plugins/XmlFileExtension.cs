using OC.Assistant.Common;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

/// <summary>
/// <see cref="Sdk.XmlFile"/> extension to update and load plugins.
/// </summary>
internal static class XmlFileExtension
{
    /// <summary>
    /// Gets a list of plugins from the <see cref="XmlFile"/>.
    /// <returns>A <see cref="List{T}"/> of type <see cref="Plugin"/>.</returns>
    /// </summary>
    public static List<Plugin> LoadPlugins(this XmlFile xmlFile)
    {
        var plugins = new List<Plugin>();

        foreach (var xPlugin in xmlFile.PluginElements())
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