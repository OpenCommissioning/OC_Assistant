using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Common;

/// <summary>
/// Available plugins.
/// </summary>
internal class PluginRegister
{
    private static readonly Lazy<PluginRegister> LazyInstance = new(() => new PluginRegister());
    private readonly List<TypeInfo> _plugins = [];
    private readonly List<TypeInfo> _extensions = [];
    
    /// <summary>
    /// The list of available plugins.
    /// </summary>
    public static IReadOnlyCollection<TypeInfo> Plugins => LazyInstance.Value._plugins;
    
    /// <summary>
    /// The list of available extensions.
    /// </summary>
    public static IReadOnlyCollection<TypeInfo> Extensions => LazyInstance.Value._extensions;
    
    /// <summary>
    /// The full path of the search directory. 
    /// </summary>
    public static string SearchPath { get; } = Path.GetFullPath(@".\Plugins");
    
    /// <summary>
    /// Gets the plugin by the given type name.
    /// </summary>
    /// <param name="typeName">The type name of the plugin.</param>
    /// <returns>The <see cref="TypeInfo"/> of the plugin if any, otherwise <c>null</c>.</returns>
    public static TypeInfo? GetByTypeName(string? typeName)
    {
        return Plugins.FirstOrDefault(x => x.Type.Name == typeName);
    }

    /// <summary>
    /// The private constructor.
    /// </summary>
    private PluginRegister()
    {
        if (LazyInstance.IsValueCreated) return;
        Initialize();
    }
    
    /// <summary>
    /// Adds found extensions into the given menu.
    /// </summary>
    public static void ImplementExtensions(Menu menu)
    {
        foreach (var package in Extensions)
        {
            try
            {
                if (Activator.CreateInstance(package.Type, AppControl.Instance) is not MenuItem menuItem) continue;
                menu.Items.Insert(1, menuItem);
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(PluginRegister), e.Message);
            }
        }
    }

    /// <summary>
    /// Tries to load available plugins depending on the current environment (debug or release). 
    /// </summary>
    private void Initialize()
    {
        _plugins.Clear();
        
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
    /// Loads the plugin from the given plugin file.
    /// </summary>
    /// <param name="pluginFile">The path to the plugin file.</param>
    private void Load(string pluginFile)
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
            
            foreach (var type in assembly.ExportedTypes)
            {
                if (type.BaseType == typeof(PluginBase))
                {
                    if (_plugins.Any(x => x.Type.FullName == type.FullName)) continue;
                    _plugins.Add(new TypeInfo(type, repositoryUrl, repositoryType));
                }
                
                if (type is {IsClass: true, IsAbstract: false, Name: "MainMenu"} &&
                    typeof(MenuItem).IsAssignableFrom(type) &&
                    type.GetConstructor([typeof(IAppControl)]) is not null)
                {
                    if (_extensions.Any(x => x.Type.FullName == type.FullName)) continue;
                    _extensions.Add(new TypeInfo(type, repositoryUrl, repositoryType));
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(PluginRegister), e.Message);
        }
    }
}