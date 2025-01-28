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
    private const string SCANNER_TOOL = "dsian.TcPnScanner.CLI";
    private bool _isScannerInstalled;

    private readonly Control _control = new (SCANNER_TOOL);

    public Menu()
    {
        InitializeComponent();
        _control.IsEnabledChanged += (_, _) => IsEnabled = _control.IsEnabled;
        Task.Run(IsScannerInstalled);
    }
        
    private void ScanOnClick(object sender, RoutedEventArgs e)
    {
        var settingsView = new SettingsView();
        var result = Theme.MessageBox.Show("Scan Profinet", settingsView, MessageBoxButton.OKCancel, MessageBoxImage.None);
        if (result != MessageBoxResult.OK) return;
        _control.StartCapture(settingsView.Settings);
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
        
        var update = new MenuItem {Header = $"Update {SCANNER_TOOL}"};
        update.Click += UpdateOnClick;
        menu.Items.Add(update);
        
        var uninstall = new MenuItem {Header = $"Uninstall {SCANNER_TOOL}"};
        uninstall.Click += UninstallOnClick;
        menu.Items.Add(uninstall);
    }
    
    private async Task IsScannerInstalled()
    {
        var lines = (await Dotnet($"tool list {SCANNER_TOOL} -g", false)).Split("\r\n");
        _isScannerInstalled = lines.Length > 1 && lines[2] != "";
    }
    
    private async void InstallOnClick(object sender, RoutedEventArgs e)
    {
        await Dotnet($"tool install {SCANNER_TOOL} -g");
        await IsScannerInstalled();
    }

    private async void UpdateOnClick(object sender, RoutedEventArgs e)
    {
        await Dotnet($"tool update {SCANNER_TOOL} -g");
        await IsScannerInstalled();
    }
    
    private async void UninstallOnClick(object sender, RoutedEventArgs e)
    {
        await Dotnet($"tool uninstall {SCANNER_TOOL} -g");
        await IsScannerInstalled();
    }

    private async Task<string> Dotnet(string arguments, bool logOutput = true)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            FileName = "dotnet",
            Arguments = arguments
        };

        BusyState.Set(this);
        process.Start();
        await process.WaitForExitAsync();
        var streamReader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8);
        var output = await streamReader.ReadToEndAsync();
        if (logOutput) Logger.LogInfo(this, output);
        BusyState.Reset(this);
        return output;
    }
}