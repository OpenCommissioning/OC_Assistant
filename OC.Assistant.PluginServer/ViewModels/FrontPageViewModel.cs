using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;

namespace OC.Assistant.PluginServer.ViewModels;

public partial class FrontPageViewModel
{
    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }

    [RelayCommand]
    private async Task Open()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("OC.Assistant.xml") { Patterns = ["*.xml"] }]
        });

        if (files.Count == 1)
            EventSystem.InvokeAppEvent("app/connect", files[0].Path.LocalPath);
    }

    [RelayCommand]
    private async Task Create()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var file = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = ".xml",
            SuggestedFileName = "OC.Assistant.xml",
            FileTypeChoices = [new FilePickerFileType("OC.Assistant.xml") { Patterns = ["*.xml"] }]
        });

        if (file is null) return;

        var path = file.Path.LocalPath;
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }

        EventSystem.InvokeAppEvent("app/connect", path);
    }
}
