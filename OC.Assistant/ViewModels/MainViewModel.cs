using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Sdk;
using OC.Assistant.Services;

namespace OC.Assistant.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public PluginEditorViewModel PluginEditorViewModel { get; }
    public PluginListViewModel PluginListViewModel { get; }

    public MainViewModel(PluginService pluginService)
    {
        PluginEditorViewModel = new PluginEditorViewModel(pluginService);
        PluginListViewModel = new PluginListViewModel(pluginService, PluginEditorViewModel);

        EventSystem.ApiDataReceived += OnApiDataReceived;
        BusyState.Changed += BusyStateOnChanged;
        
        CheckVersion();
    }

    private void BusyStateOnChanged(bool value)
    {
        IsBusy = value;
    }
    
    [ObservableProperty]
    public partial bool IsProjectConnected { get; set; }

    [ObservableProperty] 
    public partial string ProjectLabel { get; set; } = "OC.Assistant";

    [ObservableProperty]
    public partial string? Version { get; set; }

    [ObservableProperty]
    public partial bool FrontPageVisibility { get; set; } = true;

    [ObservableProperty]
    public partial bool VersionVisibility { get; set; }
    
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public IReadOnlyCollection<object?> FrontPageContents => AssemblyRegister.FrontPageContent;

    [RelayCommand]
    private void ReloadProject()
    {
        XmlFile.Instance.Reload();
    }

    [RelayCommand]
    private void CloseProject()
    {
        EventSystem.InvokeAppEvent("app/disconnect");
    }

    [RelayCommand]
    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
    
    [RelayCommand]
    private void VersionClick()
    {
        const string url = "https://github.com/opencommissioning/oc_assistant/releases/latest";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void OnApiDataReceived(string identifier, XElement payload)
    {
        switch (identifier)
        {
            case "app/connected":
                ProjectLabel = payload.Value;
                IsProjectConnected = true;
                FrontPageVisibility = false;
                break;
            case "app/disconnected":
                ProjectLabel = "OC.Assistant";
                IsProjectConnected = false;
                FrontPageVisibility = true;
                break;
            default:
                return;
        }
    }

    private async Task CheckVersion()
    {
        try
        {
            const string api = "https://api.github.com/repos/opencommissioning/oc_assistant/releases/latest";

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var current = $"v{version?.Major}.{version?.Minor}.{version?.Build}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var latest = JsonDocument
                .Parse(await client.GetStringAsync(api))
                .RootElement
                .GetProperty("tag_name")
                .GetString();

            if (current != latest)
            {
                VersionVisibility = true;
                Version = $"Release {latest} available!";
            }
        }
        catch
        {
            // ignore exceptions. fetching the latest version is not critical
        }
    }
}