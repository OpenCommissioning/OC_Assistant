using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Models;
using OC.Assistant.Sdk;
using OC.Assistant.Services;

namespace OC.Assistant.ViewModels;

public partial class PluginEditorViewModel : ObservableObject
{
    private readonly PluginService _pluginService;
    private Plugin Plugin { get; set; } = new ();
    
    public ObservableCollection<PluginParameterViewModel> Parameters { get; } = [];
    public ObservableCollection<string> AvailableTypes { get; } = [];
    public ObservableCollection<string> AvailableChannels { get; } = [];

    [ObservableProperty]
    public partial string? PluginName { get; set; }
    
    [ObservableProperty]
    public partial string? SelectedType { get; set; }
    
    [ObservableProperty]
    public partial bool TypeSelectorIsEnabled { get; set; } = true;
    
    [ObservableProperty]
    public partial string? SelectedChannel { get; set; }
    
    [ObservableProperty]
    public partial bool IsVisible { get; set; }
    
    [ObservableProperty]
    public partial bool ApplyDiscardVisibility { get; set; }

    partial void OnPluginNameChanged(string? value)
    {
        ApplyDiscardVisibility = !HideButtons;
    }
    
    partial void OnSelectedTypeChanged(string? value)
    {
        if (value == Plugin.Type?.Name) return;
        if (!Plugin.InitType(AssemblyRegister.PluginTypes.First(t => t.Name == value))) return;
        LoadParameters();
    }
    
    partial void OnSelectedChannelChanged(string? value)
    {
        if (value == Plugin.ChannelType?.Name) return;
        ApplyDiscardVisibility = !HideButtons;
    }
    
    public PluginEditorViewModel(PluginService pluginService)
    {
        _pluginService = pluginService;
        
        foreach (var type in AssemblyRegister.PluginTypes)
        {
            AvailableTypes.Add(type.Name);
        }
        
        foreach (var type in AssemblyRegister.ChannelTypes)
        {
            AvailableChannels.Add(type.Name);
        }
    }
    
    public bool HideButtons { get; init; }

    public bool HasInitialSelection
    {
        init
        {
            if (!value) return;
            IsVisible = true;
            PluginName = Plugin.Name;
            SelectedType = AssemblyRegister.PluginTypes.FirstOrDefault()?.Name;
            SelectedChannel = AssemblyRegister.ChannelTypes.FirstOrDefault()?.Name;
            LoadParameters();
        }
    }
    
    public void Select(Plugin? plugin)
    {
        if (plugin is null)
        {
            IsVisible = false;
            return;
        }
        
        IsVisible = true;
        Plugin = plugin;
        
        TypeSelectorIsEnabled = false;
        PluginName = Plugin.Name;
        SelectedType = Plugin.Type?.Name;
        SelectedChannel = Plugin.ChannelType?.Name;
        LoadParameters();
    }

    public void ResetParameters()
    {
        _pluginService.ResetPlugin(Plugin);
        Select(Plugin);
    }
        
    private void LoadParameters()
    {
        ApplyDiscardVisibility = false;

        if (!Plugin.IsValid && !Plugin.InitType(AssemblyRegister.PluginTypes.First(t => t.Name == SelectedType))) return;
        
        Parameters.Clear();
        foreach (var parameter in Plugin.PluginController.Parameter
                     .ToList().Select(p => new PluginParameterViewModel(p)))
        {
            parameter.PropertyChanged += ParameterOnPropertyChanged;
            Parameters.Add(parameter);
        }
    }

    private void ParameterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ApplyDiscardVisibility = !HideButtons;
    }

    [RelayCommand]
    public async Task<bool> Apply()
    {
        try
        {
            BusyState.Set(this);
            
            if (PluginName?.IsBasicCharacters() != true)
            {
                Logger.LogWarning(this, $"Name {PluginName} is not PLC compatible");
                return false;
            }

            if (_pluginService.Plugins.Any(plugin => plugin.Name == PluginName && !plugin.Equals(Plugin)))
            {
                Logger.LogWarning(this, $"Name {PluginName} already exists");
                return false;
            }

            var oldName = Plugin.Name != PluginName ? Plugin.Name : null;
            var oldChannel = Plugin.ChannelType;

            //Update the channel type of the selected plugin
            Plugin.ChannelType = AssemblyRegister.ChannelTypes.First(t => t.Name == SelectedChannel);

            //Update parameters of the selected plugin
            Plugin.PluginController?.Parameter.Update(Parameters);

            //Call the save method for the selected plugin
            if (!await Plugin.SaveAsync(PluginName))
            {
                Logger.LogWarning(this, $"Unable to save plugin {Plugin.Name}");
                return false;
            }

            Logger.LogInfo(this, $"Plugin '{Plugin.Name}' saved");
            ApplyDiscardVisibility = false;
            _pluginService.SavePlugin(Plugin, oldName, oldChannel);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
            return false;
        }
        finally
        {
            BusyState.Reset(this);
        }
    }
    
    [RelayCommand]
    private void Discard()
    {
        ResetParameters();
    }
}