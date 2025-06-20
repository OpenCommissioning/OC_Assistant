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
    /// The list of available plugins.
    /// </summary>
    public static List<PluginInfo> Plugins { get; } = [];

    /// <summary>
    /// The full path of the plugins search directory. 
    /// </summary>
    public static string SearchPath { get; } = Path.GetFullPath(@".\Plugins"); 

    /// <summary>
    /// Tries to load available plugins depending on the current environment (debug or executable). 
    /// </summary>
    public static void Initialize()
    {
        if (Directory.Exists(SearchPath))
        {
            Directory
                .GetFiles(SearchPath, "*.plugin", SearchOption.AllDirectories)
                .ToList().ForEach(Load);
        }
            
#if DEBUG
        /*
         Search outside the solution environment when debugging
         Path of the executable when debugging looks like this:
         <searchRoot>\<SolutionFolder>\OC.Assistant\bin\Debug\net8.0-windows\OC.Assistant.exe
         */
        try
        {
            Directory
                .GetFiles(@"..\..\..\..\..\", "*.plugin", SearchOption.AllDirectories)
                .ToList().ForEach(Load);
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(PluginRegister), e.Message);
        }
#endif
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
            var repositoryUrl = doc?.Element("RepositoryUrl")?.Value;
            var repositoryType = doc?.Element("RepositoryType")?.Value;
            var additionalDirectories = doc?.Elements("AdditionalDirectory");
            
            if (!File.Exists($"{dir}\\{dll}")) return;

            if (additionalDirectories is not null)
            {
                foreach (var additionalDirectory in additionalDirectories)
                {
                    AssemblyHelper.AddDirectory(additionalDirectory.Value, SearchOption.AllDirectories);
                }
            }
            
            AssemblyHelper.AddDirectory(dir);
            var assembly = Assembly.LoadFile($"{dir}\\{dll}");
                
            foreach (var type in assembly.ExportedTypes.Where(x => x.BaseType == typeof(PluginBase)))
            {
                if (Plugins.Any(x => x.Type.FullName == type.FullName)) continue;
                Plugins.Add(new PluginInfo(type, repositoryUrl, repositoryType));
            }
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(PluginRegister), e.Message);
        }
    }
        
    /// <summary>
    /// Gets the plugin by the given type name.
    /// </summary>
    /// <param name="typeName">The type name of the plugin.</param>
    /// <returns>The <see cref="PluginInfo"/> of the plugin if any, otherwise <c>null</c>.</returns>
    public static PluginInfo? GetByTypeName(string? typeName)
    {
        return Plugins.FirstOrDefault(x => x.Type.Name == typeName);
    }
}