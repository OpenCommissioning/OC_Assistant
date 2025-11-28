using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using OC.Assistant.Common;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.PluginSystem;

public partial class PluginManager
{
    private ObservableCollection<Plugin> Plugins { get; } = [];
    
    public PluginManager()
    {
        InitializeComponent();
        ItemsControl.ItemsSource = Plugins;
        Plugins.CollectionChanged += PluginsOnCollectionChanged;
        XmlFile.Instance.Reloaded += XmlOnReloaded;
        AppControl.Instance.Disconnected += OnDisconnect;
        AppControl.Instance.PluginStartRequested += OnPluginStartRequested;
        AppControl.Instance.PluginStopRequested += OnPluginStopRequested;
    }
    
    private void OnDisconnect()
    {
        Plugins.ToList().ForEach(x => Plugins.Remove(x));
        BtnAdd.Visibility = Visibility.Hidden;
        Editor.Clear();
    }

    private void PluginsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var plugin in e.NewItems.OfType<Plugin>())
            {
                RegisterPlugin(plugin);
            }
        }
        
        if (e.OldItems is not null)
        {
            foreach (var plugin in e.OldItems.OfType<Plugin>())
            {
                UnregisterPlugin(plugin);
            }
        }
        
        ScrollView.ScrollToEnd();
    }
    
    private void XmlOnReloaded() => Dispatcher.Invoke(Initialize);


    private void Initialize()
    {
        OnDisconnect();
        XmlFile.Instance.LoadPlugins().ForEach(Plugins.Add);
        BtnAdd.Visibility = Visibility.Visible;
    }

    private void OnPluginStopRequested(Type? clientType)
    {
        if (BusyState.IsSet) return;
        Task.Run(async () =>
        {
            foreach (var plugin in Plugins)
            {
                if (clientType is not null && plugin.ChannelType != clientType) continue;
                plugin.Stop();
                await Task.Delay(100);
            }
        });
    }

    private void OnPluginStartRequested(Type? clientType)
    {
        if (BusyState.IsSet) return;
        Task.Run(async () =>
        {
            foreach (var plugin in Plugins)
            {
                if (plugin.PluginController?.AutoStart != true) continue;
                if (clientType is not null && plugin.ChannelType != clientType) continue;
                plugin.Start();
                await Task.Delay(plugin.PluginController?.DelayAfterStart ?? 0);
            }
        });
    }
        
    private void RegisterPlugin(Plugin plugin)
    {
        plugin.OnRemove += PluginOnRemove;
        plugin.OnEdit += Select;
    }
        
    private void UnregisterPlugin(Plugin plugin)
    {
        plugin.OnRemove -= PluginOnRemove;
        plugin.OnEdit -= Select;
    }
        
    private void PluginOnRemove(Plugin plugin)
    {
        XmlFile.Instance.RemovePlugin(plugin.Name);
        Plugins.Remove(plugin);
        plugin.Dispose();
        if (plugin.IsSelected) Editor.Clear();
        if (plugin.PluginController?.IoType == IoType.None) return;
        AppControl.Instance.UpdatePlugin(null, plugin.Name);
    }

    private void EditorOnSaved(Plugin plugin, string? oldName, Type? oldChannel)
    {
        if (Plugins.FirstOrDefault(x => x == plugin) is null)
        {
            Plugins.Add(plugin);
            oldName = null;
        }
        
        XmlFile.Instance.UpdatePlugin(plugin, oldName);
        
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged && oldName is null && plugin.ChannelType == oldChannel) return;
        plugin.PluginController?.Stop();
        AppControl.Instance.UpdatePlugin(plugin.Name, oldName);
        Select(plugin);
    }
    
    private async void Select(Plugin plugin)
    {
        try
        {
            await Editor.Select(plugin, Plugins);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
    
    private async void BtnAddOnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        try
        {
            if (!await Editor.CheckUnsavedChanges()) return;
            if (AssemblyRegister.Plugins.Count == 0)
            {
                await Theme.MessageBox.Show(
                    "No plugins found", 
                    new NoPluginMessage(), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }
            var editor = new EditorWindow
            {
                Height = 400, 
                Width = 400,
                Plugins = Plugins
            };
            editor.Saved += EditorOnSaved;
        
            await Theme.MessageBox
                .Show("Add plugin", editor, MessageBoxButton.OKCancel, MessageBoxImage.None, editor.Apply);
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }

    private void EditorOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        const int limit = 200; 
        
        if (EditorColumn.Width.Value < limit)
        {
            EditorColumn.Width = new GridLength(limit);
        } 
       
        var maxWidth = ActualWidth - limit;
        if (EditorColumn.Width.Value > maxWidth)
        {
            EditorColumn.Width = new GridLength(maxWidth);
        }
    }

    private void ScrollViewOnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Editor.EditorWindow.UnsavedChanges) return;
        Editor.Clear();
    }
}