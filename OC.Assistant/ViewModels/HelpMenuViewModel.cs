using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;
using OC.Assistant.Ui;
using OC.Assistant.Views;

namespace OC.Assistant.ViewModels;

public partial class HelpMenuViewModel : ObservableObject
{
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();
    private string? ProductName => _assembly.GetName().Name;
    private string? Version => _assembly.GetName().Version?.ToString();
    private string? CompanyName =>
        (Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute)?.Company;

    public string ApplicationInfo => $"\n{ProductName}\nVersion {Version}\n{CompanyName}";

    public List<PackageInfoViewModel> LocalPackages =>
    [
        new(typeof(Logger),
            "https://www.nuget.org/packages/OC.Assistant.Sdk", "nuget"),
        new(typeof(Styles),
            "https://www.nuget.org/packages/OC.Assistant.Ui", "nuget")
    ];

    public List<PackageInfoViewModel> ExternalPackages =>
    [
        new(typeof(Microsoft.AspNetCore.Builder.WebApplication),
            "https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi", "nuget"),
        new(typeof(Serilog.ILogger),
            "https://www.nuget.org/packages/Serilog.Sinks.File", "nuget")
    ];

    public List<PackageInfoViewModel> PluginPackages =>
        AssemblyRegister.Assemblies.Select(assemblyInfo => new PackageInfoViewModel(assemblyInfo)).ToList();

    [RelayCommand]
    private void AppData()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Sdk.AppData.Path,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
    
    public bool Verbose
    {
        get => Logger.Verbose;
        set => Logger.Verbose = value;
    }

    public bool HasPlugins => PluginPackages.Count > 0;

    [RelayCommand]
    private void About() => _ = MessageBox.Show(
        new AboutPage { DataContext = this },
        MessageBoxButton.Ok,
        MessageBoxImage.Information);

    [RelayCommand]
    private void GitHubLink() => Process.Start(new ProcessStartInfo
    {
        FileName = "https://github.com/OpenCommissioning/OC_Assistant",
        UseShellExecute = true
    });
}
