using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
        
        stack.Add(new Label{Content = $"\n{ProductName}\nVersion {Version}\n{CompanyName}\n"});
        
        stack.Add(new DependencyInfo(typeof(Sdk.Logger))
        {
            Url = "https://www.nuget.org/packages/OC.Assistant.Sdk",
            UrlName = "nuget.org"
        });
        
        stack.Add(new DependencyInfo(typeof(Theme.WindowStyle))
        {
            Url = "https://www.nuget.org/packages/OC.Assistant.Theme",
            UrlName = "nuget.org"
        });
        
        stack.Add(new Label{Content = "\n\nThird party software:\n"});
        
        stack.Add(new DependencyInfo(typeof(EnvDTE.DTE))
        {
            Url = "https://www.nuget.org/packages/envdte",
            UrlName = "nuget.org"
        });
        
        stack.Add(new DependencyInfo(typeof(TwinCAT.Ads.AdsClient))
        {
            Url = "https://www.nuget.org/packages/Beckhoff.TwinCAT.Ads",
            UrlName = "nuget.org"
        });
        
        stack.Add(new DependencyInfo(typeof(TCatSysManagerLib.TcSysManager))
        {
            Url = "https://www.nuget.org/packages/TCatSysManagerLib",
            UrlName = "nuget.org"
        });
        
        stack.Add(new DependencyInfo("dsian.TcPnScanner.CLI", "latest")
        {
            Url = "https://www.nuget.org/packages/dsian.TcPnScanner.CLI",
            UrlName = "nuget.org"
        });
        
        Theme.MessageBox.Show($"About {ProductName}", content, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}