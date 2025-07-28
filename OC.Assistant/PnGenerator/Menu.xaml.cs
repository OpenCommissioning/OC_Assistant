using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

public partial class Menu
{
    private const string SCANNER_TOOL = "OC.TcPnScanner.CLI";
    private static readonly string OutputPath = $"{Path.GetTempPath()}{SCANNER_TOOL}";
    private bool _isScannerInstalled;

    private readonly Control _control = new (SCANNER_TOOL);

    public Menu()
    {
        InitializeComponent();
        ProjectState.Events.Locked += e => IsEnabled = !e;
        Task.Run(IsScannerInstalled);
    }

    private async void ScanOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsView = new SettingsView();
            var result = await Theme.MessageBox.Show(
                "Scan Profinet",
                settingsView,
                MessageBoxButton.OKCancel,
                MessageBoxImage.None, 
                settingsView.IsValid);
            if (result != MessageBoxResult.OK) return;
            _control.StartCapture(settingsView.GetSettings());
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }

    private void MenuOnSubmenuOpened(object sender, RoutedEventArgs e)
    {
        var menu = (Menu) sender;
        menu.Items.Clear();

        if (!_isScannerInstalled)
        {
            var install = new MenuItem {Header = $"Install {SCANNER_TOOL}"};
            install.Click += InstallOnClick;
            menu.Items.Add(install);
            return;
        }
        
        var scan = new MenuItem {Header = "Scan Profinet"};
        scan.Click += ScanOnClick;
        menu.Items.Add(scan);
        
        var path = new MenuItem
        {
            Header = "Show output files",
            IsEnabled = Directory.Exists(OutputPath)
        };
        path.Click += PathOnClick;
        menu.Items.Add(path);
        
        menu.Items.Add(new Separator());
        
        var update = new MenuItem {Header = $"Update {SCANNER_TOOL}"};
        update.Click += UpdateOnClick;
        menu.Items.Add(update);
        
        var uninstall = new MenuItem {Header = $"Uninstall {SCANNER_TOOL}"};
        uninstall.Click += UninstallOnClick;
        menu.Items.Add(uninstall);
    }
    
    private async Task IsScannerInstalled()
    {
        var lines = (await Powershell($"dotnet tool list {SCANNER_TOOL} -g", false)).Split("\r\n");
        _isScannerInstalled = lines.Length > 2;
    }
    
    private async void InstallOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await Powershell($"dotnet tool install {SCANNER_TOOL} -g");
            await IsScannerInstalled();
        }
        catch (Exception exception)
        {
            Logger.LogError(this, exception.Message);
        }
    }
    
    private async void UpdateOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await Powershell($"dotnet tool update {SCANNER_TOOL} -g");
            await IsScannerInstalled();
        }
        catch (Exception exception)
        {
            Logger.LogError(this, exception.Message);
        }
    }
    
    private async void UninstallOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await Powershell($"dotnet tool uninstall {SCANNER_TOOL} -g");
            await IsScannerInstalled();
        }
        catch (Exception exception)
        {
            Logger.LogError(this, exception.Message);
        }
    }

    private async Task<string> Powershell(string arguments, bool logOutput = true)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            FileName = "powershell",
            Arguments = arguments,
            Environment =
            {
                ["DOTNET_CLI_UI_LANGUAGE"] = "en"
            }
        };

        BusyState.Set(this);
        process.Start();
        await process.WaitForExitAsync();
        var streamReader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8);
        var output = (await streamReader.ReadToEndAsync()).TrimEnd('\n').TrimEnd('\r');
        if (logOutput) Logger.LogInfo(this, output);
        BusyState.Reset(this);
        return output;
    }
    
    private static void PathOnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer" , OutputPath);
    }
}