using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.ViewModels;

public partial class PluginParameterViewModel : ObservableObject, IParameter
{
    public PluginParameterViewModel(IParameter parameter)
    {
        FileFilter = parameter.FileFilter;
        Name = parameter.Name;
        Value = parameter.Value;
        ToolTip = parameter.ToolTip;
    }

    [ObservableProperty]
    public partial object? ToolTip { get; set; }

    [ObservableProperty]
    public partial bool ClearTextVisibility { get; set; } = true;

    [ObservableProperty]
    public partial bool PasswordVisibility { get; set; }

    [ObservableProperty]
    public partial bool BoolVisibility { get; set; }

    [ObservableProperty]
    public partial bool SelectFileVisibility { get; set; }

    [ObservableProperty]
    public partial string? FileFilter { get; set; }
    
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string? ClearTextValue { get; set; }

    [ObservableProperty]
    public partial string? PasswordValue { get; set; }

    [ObservableProperty]
    public partial bool BoolValue { get; set; }
    
    partial void OnFileFilterChanged(string? value)
    {
        SelectFileVisibility = value is not null && !IsPasswordFilter(value) && Value is not bool;
    }

    private static bool IsPasswordFilter(string? filter) => filter == "Password";

    public object? Value
    {
        get
        {
            if (field is bool) return BoolValue;
            return IsPasswordFilter(FileFilter) ?
                PasswordValue.ConvertTo(field?.GetType() ?? null) :
                ClearTextValue.ConvertTo(field?.GetType() ?? null);
        }
        set
        {
            field = value;
            ClearTextValue = $"{field}";

            if (field is bool boolValue)
            {
                BoolVisibility = true;
                BoolValue = boolValue;
                ClearTextVisibility = false;
                PasswordVisibility = false;
                return;
            }

            BoolVisibility = false;

            if (IsPasswordFilter(FileFilter))
            {
                PasswordVisibility = true;
                ClearTextVisibility = false;
                PasswordValue = $"{field}";
                return;
            }

            ClearTextVisibility = true;
        }
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow?.StorageProvider;
        return null;
    }

    [RelayCommand]
    private async Task SelectFile()
    {
        var provider = GetStorageProvider();
        if (provider is null) return;

        var options = new FilePickerOpenOptions { AllowMultiple = false };
        if (!string.IsNullOrEmpty(FileFilter))
        {
            options.FileTypeFilter = [new FilePickerFileType(FileFilter) { Patterns = [$"*.{FileFilter}"] }];
        }

        var files = await provider.OpenFilePickerAsync(options);
        if (files.Count == 1) Value = files[0].Path.LocalPath;
    }
}
