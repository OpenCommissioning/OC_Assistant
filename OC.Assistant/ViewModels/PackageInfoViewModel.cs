using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Models;

namespace OC.Assistant.ViewModels;

public partial class PackageInfoViewModel(Assembly assembly, string? url = null, string? urlName = null)
{
    public string Name { get; } = assembly.GetName().Name ?? "<unknown>";
    public string Version { get; } = assembly.GetName().Version?.ToString() ?? string.Empty;
    public string? Url { get; } = url;
    public string? UrlName { get; } = urlName;
    
    public bool UrlVisibility => Url is not null;
    
    public PackageInfoViewModel(AssemblyInfo assemblyInfo) : 
        this(assemblyInfo.Assembly, assemblyInfo.RepositoryUrl, assemblyInfo.RepositoryType)
    {
    }

    public PackageInfoViewModel(Type type, string? url = null, string? urlName = null) : 
        this(type.Assembly, url, urlName)
    {
    }
    
    [RelayCommand]
    private void UrlClick()
    {
        if (Url is null) return;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = Url,
            UseShellExecute = true
        });
    }
}