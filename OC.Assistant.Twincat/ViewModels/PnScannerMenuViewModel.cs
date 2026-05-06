using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;
using OC.Assistant.Ui;

namespace OC.Assistant.Twincat.ViewModels;

public partial class PnScannerMenuViewModel : ObservableObject
{
    private const string SCANNER_TOOL = "OC.TcPnScanner.CLI";
    private static readonly string OutputPath = $"{Path.GetTempPath()}{SCANNER_TOOL}";

    public PnScannerMenuViewModel()
    {
        Task.Run(IsScannerInstalled);
    }

    [RelayCommand]
    private async Task Open()
    {
        var pnScannerVm = new PnScannerViewModel();
        var pnScannerView = new Views.PnScannerWindow {DataContext = pnScannerVm};

        var result = await MessageBox.Show(
            pnScannerView,
            MessageBoxButton.OkCancel,
            MessageBoxImage.None,
            pnScannerVm.IsValid);
        
        if (!result) return;
        new PnScannerControl(SCANNER_TOOL, pnScannerVm).StartCapture();
    }

    [ObservableProperty]
    public partial bool IsNotInstalled { get; set; }
    
    [ObservableProperty]
    public partial bool IsInstalled { get; set; }

    [ObservableProperty]
    public partial bool HasLogfiles { get; set; } = Directory.Exists(OutputPath);
    
    private async Task IsScannerInstalled()
    {
        var lines = (await Powershell($"dotnet tool list {SCANNER_TOOL} -g", false)).Split("\r\n");
        IsInstalled = lines.Length > 2;
        IsNotInstalled = !IsInstalled;
    }
    
    [RelayCommand]
    private async Task Install()
    {
        await Powershell($"dotnet tool install {SCANNER_TOOL} -g");
        await IsScannerInstalled();
    }
    
    [RelayCommand]
    private async Task Update()
    {
        await Powershell($"dotnet tool update {SCANNER_TOOL} -g");
        await IsScannerInstalled();
    }
    
    [RelayCommand]
    private async Task Uninstall()
    {
        await Powershell($"dotnet tool uninstall {SCANNER_TOOL} -g");
        await IsScannerInstalled();
    }
    
    [RelayCommand]
    private void Logfiles()
    {
        Process.Start("explorer" , OutputPath);
    }

    private async Task<string> Powershell(string arguments, bool logOutput = true)
    {
        try
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
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return string.Empty;
        }
    }
}