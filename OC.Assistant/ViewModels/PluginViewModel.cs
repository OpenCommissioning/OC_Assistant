using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Models;
using OC.Assistant.Ui;

namespace OC.Assistant.ViewModels;

public partial class PluginViewModel : ObservableObject
{
    public Plugin Model { get; set; }
    public string Name => Model.Name;
    public string Type => Model.Type?.Name ?? "<unknown>";
    public string ChannelType => Model.ChannelType?.Name ?? "<unknown>";
    
    [ObservableProperty]
    public partial bool StartStopButtonIsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool Run { get; set; }
    
    [ObservableProperty]
    public partial bool Idle { get; set; } = true;
    
    [ObservableProperty]
    public partial bool Starting { get; set; }
    
    [ObservableProperty]
    public partial bool Stopping { get; set; }

    private void PluginOnSaved()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Type));
        OnPropertyChanged(nameof(ChannelType));
    }
    
    public PluginViewModel(Plugin plugin)
    {
        Model = plugin;
        Model.StateChanged += PluginOnStateChanged;
        Model.Saved += PluginOnSaved;
    }

    private void PluginOnStateChanged(Plugin.State state)
    {
        switch (state)
        {
            case Plugin.State.Starting:
                StartStopButtonIsEnabled = false;
                Run = false;
                Idle = false;
                Starting = true;
                Stopping = false;
                return;
            case Plugin.State.Run:
                StartStopButtonIsEnabled = true;
                Run = true;
                Idle = false;
                Starting = false;
                Stopping = false;
                return;
            case Plugin.State.Stopping:
                StartStopButtonIsEnabled = false;
                Run = false;
                Idle = false;
                Starting = false;
                Stopping = true;
                return;
            case Plugin.State.Idle:
            default:
                StartStopButtonIsEnabled = true;
                Run = false;
                Idle = true;
                Starting = false;
                Stopping = false;
                return;
        }   
    }

    [RelayCommand]
    private void StartStop()
    {
        if (Model.PluginController?.IsRunning == true)
        {
            Model.PluginController.Stop();
            return;
        }

        Model.PluginController?.Start();
    }

    [RelayCommand]
    private async Task Remove()
    {
        if (Model.PluginController?.IsRunning == true) return;
        if (await MessageBox.Show(
                $"Delete {Name}?", MessageBoxButton.OkCancel, MessageBoxImage.Warning))
        {
            OnRemove?.Invoke(Model);
        }
    }

    public event Action<Plugin>? OnRemove;
}