using OC.Assistant.Common;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

/// <summary>
/// <see cref="Sdk.XmlFile"/> extension to update and load plugins.
/// </summary>
internal static class XmlFileExtension
{
    extension(XmlFile xmlFile)
    {
        /// <summary>
        /// Gets a list of plugins from the <see cref="XmlFile"/>.
        /// <returns>A <see cref="List{T}"/> of type <see cref="Plugin"/>.</returns>
        /// </summary>
        public List<Plugin> LoadPlugins()
        {
            var plugins = new List<Plugin>();

            foreach (var xPlugin in xmlFile.GetPluginElements())
            {
                var pluginInfo = PluginRegister.GetByTypeName(xPlugin.Type);
                if (pluginInfo is null)
                {
                    Logger.LogWarning(typeof(XmlFile), 
                        $"Plugin for type '{xPlugin.Type}' not found in directory {PluginRegister.SearchPath}");
                    continue;
                }
                var clientType = PluginRegister.ClientTypes.FirstOrDefault(t => t.Name == xPlugin.ClientType);
                plugins.Add(new Plugin(xPlugin.Name, pluginInfo.Type, clientType, xPlugin.Parameter));
            }
            
            return plugins;
        }
        
        /// <summary>
        /// Removes all <see cref="XPlugin"/> elements by the given name.
        /// </summary>
        public void RemovePlugin(string? name)
        {
            if (name is null) return;
        
            foreach (var xPlugin in xmlFile.GetPluginElements().Where(x => x.Name == name))
            {
                xPlugin.Element?.Remove();
            }
        
            xmlFile.Save();
        }
    
        /// <summary>
        /// Updates or adds the given <see cref="Plugin"/> to the <see cref="XmlFile"/>.
        /// </summary>
        public void UpdatePlugin(IPlugin plugin, string? oldName = null)
        {
            xmlFile.RemovePlugin(oldName ?? plugin.Name);
            xmlFile.Plugins.Add(new XPlugin(plugin).Element);
            xmlFile.Save();
        }
    }
}