using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Core;

internal class PackageRegister : List<TypeInfo>
{
    private static readonly Lazy<PackageRegister> LazyInstance = new(() => []);
    
    /// <summary>
    /// The list of available packages.
    /// </summary>
    public static IReadOnlyCollection<TypeInfo> Packages => LazyInstance.Value;
    
    /// <summary>
    /// The full path of the packages search directory. 
    /// </summary>
    public string SearchPath { get; } = Path.GetFullPath(@".\Packages");

    /// <summary>
    /// The private constructor.
    /// </summary>
    private PackageRegister()
    {
        Initialize();
    }
    
    /// <summary>
    /// Tries to load available packages depending on the current environment (debug or release). 
    /// </summary>
    private void Initialize()
    {
        Clear();
        
        if (Directory.Exists(SearchPath))
        {
            Directory
                .GetFiles(SearchPath, "*.package", SearchOption.AllDirectories)
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
                .GetFiles(@"..\..\..\..\..\", "*.package", SearchOption.AllDirectories)
                .ToList().ForEach(Load);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
#endif
    }
    
    /// <summary>
    /// Loads the package from the given package file.
    /// </summary>
    /// <param name="packageFile">The path to the package file.</param>
    private void Load(string packageFile)
    {
        try
        {
            var filePath = Path.GetFullPath(packageFile);
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

            foreach (var type in assembly.ExportedTypes.Where(t =>
                         t is {IsClass: true, IsAbstract: false, Name: "MainMenu"} &&
                         typeof(MenuItem).IsAssignableFrom(t) &&
                         t.GetConstructor([typeof(IAppControl)]) is not null))
            {
                if (this.Any(x => x.Type.FullName == type.FullName)) continue;
                Add(new TypeInfo(type, repositoryUrl, repositoryType));
            }
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
}