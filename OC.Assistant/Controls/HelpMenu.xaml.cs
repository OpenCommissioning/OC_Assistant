using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Common;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

internal partial class HelpMenu
{
    public HelpMenu()
    {
        InitializeComponent();
    }
    
    private static Assembly Assembly => typeof(MainWindow).Assembly;
    private static string? ProductName => Assembly.GetName().Name;
    private static string? Version => Assembly.GetName().Version?.ToString();
    private static string? CompanyName
    {
        get
        {
            var company = Attribute
                .GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            return company?.Company;
        }
    }
    
    private class GitHubLink : Button
    {
        public GitHubLink()
        {
            const string url = "https://github.com/OpenCommissioning/OC_Assistant";
            Style = Application.Current.FindResource("LinkButton") as Style;
            Margin = new Thickness(0, 10, 0, 20);
            Content = "Assistant github page";
            Click += (_, _) => Process.Start(new ProcessStartInfo {FileName = url, UseShellExecute = true});
        }
    }
    
    private void AppDataOnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe" , AppData.Path);
    }

    private void VerboseOnClick(object sender, RoutedEventArgs e)
    {
        Logger.Verbose = ((CheckBox) sender).IsChecked == true;
    }
    
    private void AboutOnClick(object sender, RoutedEventArgs e)
    {
        var content = new StackPanel();
        var stack = content.Children;
        
        stack.Add(new Label {Content = $"\n{ProductName}\nVersion {Version}\n{CompanyName}"});

        stack.Add(new GitHubLink());
        
        stack.Add(new DependencyInfo(typeof(Logger))
        {
            Url = "https://github.com/OpenCommissioning/OC_Assistant_Sdk",
            UrlName = "github"
        });
        
        stack.Add(new DependencyInfo(typeof(Theme.Window))
        {
            Url = "https://github.com/OpenCommissioning/OC_Assistant_Theme",
            UrlName = "github"
        });
        
        Addons(stack);
        AddThirdParty(stack);
        AddPlugins(stack);
        
        _ = Theme.MessageBox.Show($"About {ProductName}", content, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private static void Addons(UIElementCollection stack)
    {
        var packages = PluginRegister.Extensions.DistinctBy(x => x.Type.Assembly.FullName);
        
        foreach (var package in packages)
        {
            stack.Add(new DependencyInfo(package.Type)
            {
                Url = package.RepositoryUrl,
                UrlName = package.RepositoryType
            });
        }
    }
    
    private static void AddPlugins(UIElementCollection stack)
    {
        var plugins = PluginRegister.Plugins.DistinctBy(x => x.Type.Assembly.FullName).ToArray();
        if (plugins.Length == 0) return;
        
        stack.Add(new Label{Content = "\n\nPlugins:\n"});
        
        foreach (var plugin in plugins)
        {
            stack.Add(new DependencyInfo(plugin.Type)
            {
                Url = plugin.RepositoryUrl,
                UrlName = plugin.RepositoryType
            });
        }
    }

    private static void AddThirdParty(UIElementCollection stack)
    {
        stack.Add(new Label{Content = "\n\nNuget packages:\n"});
        
        stack.Add(new DependencyInfo(typeof(Microsoft.AspNetCore.Builder.WebApplication))
        {
            Url = "https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi",
            UrlName = "nuget"
        });
        
        stack.Add(new DependencyInfo(typeof(Serilog.ILogger))
        {
            Url = "https://www.nuget.org/packages/Serilog.Sinks.File",
            UrlName = "nuget"
        });
    }
}