using OC.Assistant.Models;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Services;

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
                var pluginType = AssemblyRegister.PluginTypes.FirstOrDefault(x => x.Name == xPlugin.Type);
                if (pluginType is null)
                {
                    Logger.LogWarning(typeof(XmlFile), 
                        $"Plugin for type '{xPlugin.Type}' not found in directory {AssemblyRegister.SearchPath}");
                    continue;
                }
                var clientType = AssemblyRegister.ChannelTypes.FirstOrDefault(t => t.Name == xPlugin.ChannelType);
                plugins.Add(new Plugin(xPlugin.Name, pluginType, clientType, xPlugin.Parameter));
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
        
        /// <summary>
        /// Loads the parameters from the <see cref="XmlFile"/> and updates the given plugin.
        /// </summary>
        public void LoadPluginParameters(IPlugin plugin)
        {
            var xPlugin = xmlFile.GetPluginElements().FirstOrDefault(x => x.Name == plugin.Name);
            if (xPlugin is null) return;
            plugin.PluginController?.Parameter.Update(xPlugin.Parameter);
        }
    }
}