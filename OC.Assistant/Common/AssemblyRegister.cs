using System.IO;
using System.Reflection;
using System.Xml.Linq;
using OC.Assistant.PluginSystem;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Common;

/// <summary>
/// Static class that manages the loading of external assemblies.
/// </summary>
internal class AssemblyRegister
{
    private static readonly Lazy<AssemblyRegister> LazyInstance = new(() => new AssemblyRegister());
    private readonly List<Type> _plugins = [];
    private readonly List<Type> _extensions = [];
    private readonly List<Type> _channels = [];
    private readonly List<AssemblyInfo> _assemblies = [];
    
    /// <summary>
    /// The list of available types with baseType <see cref="PluginBase"/>.
    /// </summary>
    public static IReadOnlyCollection<Type> Plugins => LazyInstance.Value._plugins;
    
    /// <summary>
    /// The list of available types with baseType <see cref="ExtensionBase"/>.
    /// </summary>
    public static IReadOnlyCollection<Type> Extensions => LazyInstance.Value._extensions;
    
    /// <summary>
    /// The list of available types with baseType <see cref="ChannelBase"/>.
    /// </summary>
    public static IReadOnlyCollection<Type> Channels => LazyInstance.Value._channels;
    
    /// <summary>
    /// The list of external assemblies.
    /// </summary>
    public static IReadOnlyCollection<AssemblyInfo> Assemblies => LazyInstance.Value._assemblies;
    
    /// <summary>
    /// The full path of the search directory. 
    /// </summary>
    public static string SearchPath { get; } = Path.GetFullPath(@".\Plugins");
    
    private AssemblyRegister()
    {
        if (LazyInstance.IsValueCreated) return;
        Initialize();
    }
    
    private void ActivateExtensions()
    {
        foreach (var extension in _extensions)
        {
            try
            {
                Activator.CreateInstance(extension, AppControl.Instance);
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(AssemblyRegister), e.Message);
            }
        }
    }

    /// <summary>
    /// Tries to load available assemblies, depending on the current environment (debug or release). 
    /// </summary>
    private void Initialize()
    {
        _channels.Add(typeof(TcpIpChannel));
        
        if (Directory.Exists(SearchPath))
        {
            Directory.GetFiles(SearchPath, "*.plugin", SearchOption.AllDirectories)
                .ToList()
                .ForEach(Load);
        }
            
#if DEBUG
        /*
         Search outside the solution environment when debugging
         Path of the executable when debugging looks like this:
         <searchRoot>\<SolutionFolder>\OC.Assistant\bin\Debug\net8.0-windows\OC.Assistant.exe
         */
        try
        {
            Directory.GetFiles(@"..\..\..\..\..\", "*.plugin", SearchOption.AllDirectories)
                .ToList()
                .ForEach(Load);
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(AssemblyRegister), e.Message);
        }
#endif
        
        ActivateExtensions();
    }
    
    /// <summary>
    /// Loads any suitable type from the given plugin file.
    /// </summary>
    /// <param name="pluginFile">The path of the plugin file.</param>
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
            var assembly = Assembly.LoadFrom($"{dir}\\{dll}");

            if (_assemblies.All(x => x.Assembly != assembly))
            {
                _assemblies.Add(new AssemblyInfo(assembly, repositoryUrl, repositoryType));
            }
            
            foreach (var type in assembly.ExportedTypes)
            {
                if (type.BaseType == typeof(PluginBase))
                {
                    if (_plugins.Any(x => x.FullName == type.FullName)) continue;
                    _plugins.Add(type);
                }
                
                if (type.BaseType == typeof(ExtensionBase))
                {
                    if (_extensions.Any(x => x.FullName == type.FullName)) continue;
                    _extensions.Add(type);
                }
                
                if (type.BaseType == typeof(ChannelBase))
                {
                    if (_channels.Any(x => x.FullName == type.FullName)) continue;
                    _channels.Add(type);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(AssemblyRegister), e.Message);
        }
    }
}