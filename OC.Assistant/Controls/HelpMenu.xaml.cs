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

        stack.Add(new GitHubLink {Margin = new Thickness(0,10,0,20)});
        
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
        
        AddThirdParty(stack);
        AddAssemblies(stack);
        
        _ = Theme.MessageBox.Show($"About {ProductName}", content, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private static void AddAssemblies(UIElementCollection stack)
    {
        if (AssemblyRegister.Assemblies.Count == 0) return;
        
        stack.Add(new Label{Content = "\n\nPlugins:\n"});
        
        foreach (var assemblyInfo in AssemblyRegister.Assemblies)
        {
            stack.Add(new DependencyInfo(assemblyInfo.Assembly)
            {
                Url = assemblyInfo.RepositoryUrl,
                UrlName = assemblyInfo.RepositoryType
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