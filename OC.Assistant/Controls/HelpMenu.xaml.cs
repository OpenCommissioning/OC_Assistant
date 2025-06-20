using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            Cursor = Cursors.Hand;
            Content = url;
            Click += (_, _) => Process.Start(new ProcessStartInfo {FileName = url, UseShellExecute = true});
        }
    }
    
    private void AppDataOnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe" , Core.AppData.Path);
    }

    private void VerboseOnClick(object sender, RoutedEventArgs e)
    {
        Sdk.Logger.Verbose = ((CheckBox) sender).IsChecked == true;
    }
    
    private void AboutOnClick(object sender, RoutedEventArgs e)
    {
        var content = new StackPanel();
        var stack = content.Children;
        
        stack.Add(new Label {Content = $"\n{ProductName}\nVersion {Version}\n{CompanyName}"});

        stack.Add(new GitHubLink());
        
        stack.Add(new DependencyInfo(typeof(Sdk.Logger))
        {
            Url = "https://github.com/OpenCommissioning/OC_Assistant_Sdk",
            UrlName = "github"
        });
        
        stack.Add(new DependencyInfo(typeof(Theme.WindowStyle))
        {
            Url = "https://github.com/OpenCommissioning/OC_Assistant_Theme",
            UrlName = "github"
        });
        
        AddThirdParty(stack);
        AddPlugins(stack);
        
        Theme.MessageBox.Show($"About {ProductName}", content, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private static void AddPlugins(UIElementCollection stack)
    {
        var plugins = Plugins.PluginRegister.Plugins.DistinctBy(x => x.Type.Assembly.FullName).ToArray();
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
        stack.Add(new Label{Content = "\n\nThird party software:\n"});
        
        stack.Add(new DependencyInfo(typeof(EnvDTE.DTE))
        {
            Url = "https://www.nuget.org/packages/envdte",
            UrlName = "nuget"
        });
        
        stack.Add(new DependencyInfo(typeof(Microsoft.WindowsAPICodePack.Dialogs.DialogControl))
        {
            Url = "https://www.nuget.org/packages/Microsoft-WindowsAPICodePack-Core",
            UrlName = "nuget"
        });
        
        stack.Add(new DependencyInfo(typeof(TwinCAT.Ads.AdsClient))
        {
            Url = "https://www.nuget.org/packages/Beckhoff.TwinCAT.Ads",
            UrlName = "nuget"
        });
        
        stack.Add(new DependencyInfo(typeof(TCatSysManagerLib.TcSysManager))
        {
            Url = "https://www.nuget.org/packages/TCatSysManagerLib",
            UrlName = "nuget"
        });
        
        stack.Add(new DependencyInfo("dsian.TcPnScanner.CLI", "")
        {
            Url = "https://www.nuget.org/packages/dsian.TcPnScanner.CLI",
            UrlName = "nuget"
        });
    }
}