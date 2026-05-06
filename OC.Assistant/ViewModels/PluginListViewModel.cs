using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.Models;
using OC.Assistant.Services;
using OC.Assistant.Ui;
using OC.Assistant.Views;

namespace OC.Assistant.ViewModels;

public partial class PluginListViewModel : ObservableObject
{
    private readonly PluginService _pluginService;
    private readonly PluginEditorViewModel _editor;
    
    public ObservableCollection<PluginViewModel> Plugins { get; } = [];

    [ObservableProperty] 
    public partial PluginViewModel? SelectedPlugin { get; set; }
    
    public PluginListViewModel(PluginService pluginService, PluginEditorViewModel editorViewModel)
    {
        pluginService.Plugins.CollectionChanged += PluginsOnCollectionChanged;
        _pluginService = pluginService;
        _editor = editorViewModel;
    }
    
    [RelayCommand]
    private static void OpenGitHubLink()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/OpenCommissioning/OC_Assistant",
            UseShellExecute = true
        });
    }
    
    [RelayCommand]
    private async Task Add()
    {
        if (AssemblyRegister.PluginTypes.Count == 0)
        {
            await MessageBox.Show(
                new NoPluginMessage { DataContext = this },
                MessageBoxButton.Ok,
                MessageBoxImage.Warning);
            return;
        }

        var editorViewModel = new PluginEditorViewModel(_pluginService)
        {
            HideButtons = true,
            HasInitialSelection = true
        };

        await MessageBox.Show(
            new PluginEditor { Height = 400, Width = 400, DataContext = editorViewModel },
            MessageBoxButton.OkCancel,
            MessageBoxImage.None,
            editorViewModel.Apply);
    }
    
    partial void OnSelectedPluginChanged(PluginViewModel? oldValue, PluginViewModel? newValue)
    {
        if (newValue is null)
        {
            _editor.Select(null);
            return;
        }
        
        _editor.Select(newValue.Model);
    }

    private void PluginsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_pluginService.Plugins.Count == 0 && e.Action == NotifyCollectionChangedAction.Reset)
        {
            Plugins.Clear();
            return;
        }
        
        if (e.OldItems is not null)
        {
            foreach (var plugin in e.OldItems.OfType<Plugin>())
            {
                var pluginViewModel = Plugins.FirstOrDefault(x => x.Model.Equals(plugin));
                if (pluginViewModel is null) continue;
                Plugins.Remove(pluginViewModel);
                pluginViewModel.OnRemove -= PluginOnRemove;
            }
        }
        
        if (e.NewItems is not null)
        {
            foreach (var plugin in e.NewItems.OfType<Plugin>())
            {
                var pluginViewModel = new PluginViewModel(plugin);
                Plugins.Add(pluginViewModel);
                pluginViewModel.OnRemove += PluginOnRemove;
            }
        }
    }
    
    private void PluginOnRemove(Plugin plugin)
    {
        if (SelectedPlugin?.Model == plugin) SelectedPlugin = null;
        _pluginService.RemovePlugin(plugin);
    }
}