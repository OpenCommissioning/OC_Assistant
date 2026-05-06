using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;
using OC.Assistant.Models;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;
using OC.Assistant.Twincat;

namespace OC.Assistant;

/// <summary>
/// Static class that manages the loading of external assemblies.
/// </summary>
internal class AssemblyRegister
{
    private static readonly ConcurrentBag<string> Directories = [];
    private static readonly Lazy<AssemblyRegister> LazyInstance = new(() => new AssemblyRegister());
    private readonly List<AssemblyInfo> _assemblies = [];
    private readonly List<Type> _pluginTypes = [];
    private readonly List<Type> _channelTypes = [];
    private readonly List<object?> _menus = [];
    private readonly List<object?> _frontPageContent = [];
    
    /// <summary>
    /// The list of available types with baseType <see cref="PluginBase"/>.
    /// </summary>
    public static IReadOnlyCollection<Type> PluginTypes => LazyInstance.Value._pluginTypes;
    
    /// <summary>
    /// The list of available types with baseType <see cref="ChannelBase"/>.
    /// </summary>
    public static IReadOnlyCollection<Type> ChannelTypes => LazyInstance.Value._channelTypes;
    
    /// <summary>
    /// The list of available menus.
    /// </summary>
    public static IReadOnlyCollection<object?> Menus => LazyInstance.Value._menus;
    
    /// <summary>
    /// The list of available front page content.
    /// </summary>
    public static IReadOnlyCollection<object?> FrontPageContent => LazyInstance.Value._frontPageContent;
    
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

    /// <summary>
    /// Tries to load available assemblies, depending on the current environment (debug or release). 
    /// </summary>
    private void Initialize()
    {
        if (App.Settings.EnablePluginServer)
        {
            Activate(typeof(PluginServer.TcpIpServer).Assembly);
        }
        
        if (TcDte.IsTwincat3Installed())
        {
            Activate(typeof(TcDte).Assembly);
        }
        
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
            if (dll is null || dir is null) return;
            
            var dllPath = Path.Combine(dir, dll);
            
            if (!File.Exists(dllPath)) return;

            if (additionalDirectories is not null)
            {
                foreach (var additionalDirectory in additionalDirectories)
                {
                    AddDirectory(additionalDirectory.Value, SearchOption.AllDirectories);
                }
            }
            
            AddDirectory(dir);
            var assembly = Assembly.LoadFrom(dllPath);

            if (_assemblies.Any(x => x.Assembly == assembly)) return;
            _assemblies.Add(new AssemblyInfo(assembly, repositoryUrl, repositoryType));
            
            Activate(assembly);
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(AssemblyRegister), e.Message);
        }
    }

    /// <summary>
    /// Gets all types from the given assembly and adds them to the corresponding lists.
    /// </summary>
    /// <param name="assembly">The assembly to get the types from.</param>
    private void Activate(Assembly assembly)
    {
        try
        {
            foreach (var type in assembly.ExportedTypes.Where(x => x is {IsClass: true, IsAbstract: false}))
            {
                if (type.GetCustomAttributes<CustomMenu>().Any())
                {
                    _menus.Add(Activator.CreateInstance(type));
                    continue;
                }
                
                if (type.GetCustomAttributes<CustomFrontPage>().Any())
                {
                    _frontPageContent.Add(Activator.CreateInstance(type));
                    continue;
                }
                
                if (type.BaseType == typeof(PluginBase))
                {
                    if (_pluginTypes.Any(x => x.FullName == type.FullName)) continue;
                    _pluginTypes.Add(type);
                    continue;
                }
                
                if (type.BaseType == typeof(ChannelBase))
                {
                    if (_channelTypes.Any(x => x.FullName == type.FullName)) continue;
                    _channelTypes.Add(type);
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(AssemblyRegister), e.Message);
        }
    }
    
    /// <summary>
    /// Adds a new directory to search dlls.
    /// <param name="directory">The path of the directory.</param>
    /// <param name="searchOption">The <see cref="SearchOption"/>.</param>
    /// </summary>
    private static void AddDirectory(string? directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (!Directory.Exists(directory) || Directories.Any(x => x == directory)) return;
        Directories.Add(directory);
        
        AppDomain.CurrentDomain.AssemblyResolve += (_, resolveEventArgs) =>
        {
            var assemblyFile = $"{resolveEventArgs.Name.Split(',')[0]}.dll";
            
            return Directory
                .GetFiles(directory, assemblyFile, searchOption)
                .Select(Assembly.LoadFile)
                .LastOrDefault();
        };
    }
}