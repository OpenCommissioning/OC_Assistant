using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

/// <summary>
/// <see cref="Core.XmlFile"/> extension to update and load plugins.
/// </summary>
internal static class XmlFileExtension
{
    /// <summary>
    /// Gets the plugin elements as <see cref="XPlugin"/>.
    /// </summary>
    public static IEnumerable<XPlugin> PluginElements(this XmlFile xmlFile) 
        => xmlFile.Plugins.Elements().Select(x => new XPlugin(x));

    /// <summary>
    /// Retrieves the <see cref="XPlugin"/> by the given name.
    /// </summary>
    /// <returns>The first <see cref="XPlugin"/> corresponding to the name, if any.</returns>
    public static XPlugin? GetPlugin(this XmlFile xmlFile, string name) 
        => xmlFile.PluginElements().FirstOrDefault(x => x.Name == name);

    /// <summary>
    /// Removes all <see cref="XPlugin"/> elements by the given name.
    /// </summary>
    public static void RemovePlugin(this XmlFile xmlFile, string? name)
    {
        if (name is null) return;
        
        foreach (var xPlugin in xmlFile.PluginElements().Where(x => x.Name == name))
        {
            xPlugin.Element?.Remove();
        }
        
        xmlFile.Save();
    }
    
    /// <summary>
    /// Updates or adds the given <see cref="Plugin"/> to the <see cref="XmlFile"/>.
    /// </summary>
    public static void UpdatePlugin(this XmlFile xmlFile, Plugin plugin)
    {
        xmlFile.RemovePlugin(plugin.Name);
        xmlFile.Plugins.Add(new XPlugin(plugin).Element);
        xmlFile.Save();
    }
        
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