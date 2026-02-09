using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace OC.Assistant.Core;

/// <summary>
/// Static <see cref="Assembly"/> helper.
/// </summary>
public static class AssemblyHelper
{
    private static readonly ConcurrentBag<string> Directories = [];
        
    /// <summary>
    /// Adds a new directory to search dlls.
    /// <param name="directory">The path of the directory.</param>
    /// <param name="searchOption">The <see cref="SearchOption"/>.</param>
    /// </summary>
    public static void AddDirectory(string? directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
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