using System.IO;
using System.Reflection;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

/// <summary>
/// Available plugins.
/// </summary>
internal static class PluginRegister
{
    /// <summary>
    /// The list of available plugin types.
    /// </summary>
    public static List<Type> Types { get; } = [];

    /// <summary>
    /// Tries to load available plugins depending on the current environment (debug or executable). 
    /// </summary>
    public static void Initialize()
    {
        
/*#if DEBUG
        foreach (var pluginFile in Directory.GetFiles(@"..\..\..\..\Plugins", "*.plugin", SearchOption.AllDirectories))
        {
            Load(pluginFile);
        }
#endif*/
        
        foreach (var pluginFile in Directory.GetFiles(Environment.CurrentDirectory, "*.plugin", SearchOption.AllDirectories))
        {
            Load(pluginFile);
        }
    }
    
    /// <summary>
    /// Loads the plugin by the given plugin file.
    /// </summary>
    /// <param name="pluginFile">The path to the plugin file.</param>
    private static void Load(string pluginFile)
    {
        try
        {
            var filePath = Path.GetFullPath(pluginFile);
            var dir = Path.GetDirectoryName(filePath);
            var doc = XDocument.Load(filePath).Root;
            var dll = doc?.Element("AssemblyFile")?.Value;
            var additional = doc?.Elements("AdditionalDirectory");

            if (additional is not null)
            {
                foreach (var additionalDirectory in additional)
                {
                    AssemblyHelper.AddDirectory(additionalDirectory.Value, SearchOption.AllDirectories);
                }
            }
            
            AssemblyHelper.AddDirectory(dir);
            var assembly = Assembly.LoadFile($"{dir}\\{dll}");
                
            foreach (var type in assembly.ExportedTypes.Where(x => x.BaseType == typeof(PluginBase)))
            {
                if (Types.Any(x => x.FullName == type.FullName)) continue;
                Types.Add(type);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(PluginRegister), e.Message);
        }
    }
        
    /// <summary>
    /// Gets the plugin by the given name.
    /// </summary>
    /// <param name="name">The name of the plugin type.</param>
    /// <returns>The type of the plugin if available, otherwise default.</returns>
    public static Type? GetTypeByName(string? name)
    {
        return Types.FirstOrDefault(x => x.Name == name);
    }
}