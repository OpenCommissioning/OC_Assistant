using System.Net.NetworkInformation;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat.ViewModels;

public partial class PnScannerViewModel : ObservableObject, IPnScannerSettings
{
    private Dictionary<string, NetworkInterface> Adapters { get; } = [];

    public PnScannerViewModel()
    {
        GetAdapters();
    }

    [ObservableProperty]
    public partial string PnName { get; set; } = "PNIO";

    [ObservableProperty]
    public partial string? HwFile { get; set; }

    [ObservableProperty]
    public partial string? GsdFolder { get; set; }
    
    [ObservableProperty]
    public partial string SelectedAdapterKey { get; set; } = "";
    
    public IEnumerable<string> AdapterKeys => Adapters.Keys;

    public NetworkInterface? Adapter => Adapters.GetValueOrDefault(SelectedAdapterKey);

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(PnName))
        {
            Logger.LogError(this, "Profinet name is empty");
            return false;
        }

        if (Adapter is null)
        {
            Logger.LogError(this, "No adapter selected");
            return false;
        }

        if (HwFile is null)
        {
            Logger.LogError(this, "TIA aml file not specified");
            return false;
        }

        return true;
    }
    
    [RelayCommand]
    private async Task SelectAmlFile()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("TIA aml export") { Patterns = ["*.aml"] }]
        });

        if (files.Count == 1)
            HwFile = files[0].Path.LocalPath;
    }

    [RelayCommand]
    private async Task SelectGsdFolder()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "GSDML folder",
            AllowMultiple = false
        });

        if (folders.Count == 1)
            GsdFolder = folders[0].Path.LocalPath;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }

    private void GetAdapters()
    {
        try
        {
            Adapters.Clear();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!adapter.Description.Contains("twincat-intel", StringComparison.CurrentCultureIgnoreCase)) continue;
                Adapters.Add($"{adapter.Name} ({adapter.Description})", adapter);
            }

            OnPropertyChanged(nameof(Adapters));
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
}
