using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

public partial class PluginManager
{
    private List<Plugin> _plugins = [];
    
    public PluginManager()
    {
        InitializeComponent();
        XmlFile.Instance.Reloaded += XmlOnReloaded;
        ProjectState.Events.Disconnected += OnDisconnect;
        ProjectState.Events.StartedRunning += OnStartedRunning;
        ProjectState.Events.StoppedRunning += OnStoppedRunning;
        ProjectState.Events.Locked += OnLocked;
    }
    
    private void OnLocked(bool value)
    {
        BtnAdd.Visibility = value ? Visibility.Hidden : Visibility.Visible;
        Editor.IsEnabled = !value;
        foreach (var plugin in _plugins)
        {
            plugin.IsEnabled = !value;
        }
    }
    
    private void XmlOnReloaded() => Dispatcher.Invoke(Initialize);


    private void Initialize()
    {
        OnDisconnect();
        _plugins = XmlFile.Instance.LoadPlugins();

        foreach (var plugin in _plugins)
        {
            AddPlugin(plugin);
        }
        
        BtnAdd.Visibility = Visibility.Visible;
    }

    private void OnDisconnect()
    {
        foreach (var plugin in _plugins)
        {
            RemovePlugin(plugin);
        }
        
        BtnAdd.Visibility = Visibility.Hidden;
        HideEditor();
        _plugins.Clear();
    }

    private void OnStoppedRunning()
    {
        if (BusyState.IsSet) return;
        Task.Run(async () =>
        {
            foreach (var plugin in _plugins)
            {
                plugin.Stop();
                await Task.Delay(100);
            }
        });
    }

    private void OnStartedRunning()
    {
        if (BusyState.IsSet) return;
        Task.Run(async () =>
        {
            foreach (var plugin in _plugins.Where(x => x.PluginController?.AutoStart == true))
            {
                plugin.Start();
                await Task.Delay(plugin.PluginController?.DelayAfterStart ?? 0);
            }
        });
    }
        
    private void AddPlugin(Plugin plugin)
    {
        plugin.OnRemove += PluginOnRemove;
        plugin.OnEdit += ShowEditor;
        
        Dispatcher.Invoke(() =>
        {
            ControlPanel.Children.Remove(BtnAdd);
            ControlPanel.Children.Add(plugin);
            DockPanel.SetDock(plugin, Dock.Top);
            ControlPanel.Children.Add(BtnAdd);
            ScrollView.ScrollToEnd();
        });
    }
        
    private void RemovePlugin(Plugin plugin)
    {
        plugin.PluginController?.Stop();
        plugin.OnRemove -= PluginOnRemove;
        plugin.OnEdit -= ShowEditor;
        
        Dispatcher.Invoke(() =>
        {
            ControlPanel.Children.Remove(plugin);
        });
    }
        
    private void PluginOnRemove(Plugin plugin)
    {
        XmlFile.Instance.RemovePlugin(plugin.Name);
        _plugins.Remove(plugin);
        RemovePlugin(plugin);
        HideEditor();
        if (plugin.PluginController?.IoType == IoType.None) return;
        UpdateProject(null, plugin.Name);
    }

    private void EditorOnSaved(Plugin plugin, string? oldName)
    {
        XmlFile.Instance.UpdatePlugin(plugin);

        if (_plugins.FirstOrDefault(x => x == plugin) is null)
        {
            _plugins.Add(plugin);
            AddPlugin(plugin);
            oldName = null;
        }
        else
        {
            XmlFile.Instance.RemovePlugin(oldName);
            plugin.PluginController?.Stop();
        }
        
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged && oldName is null) return;
        UpdateProject(plugin.Name, oldName);
    }
    
    private async void ShowEditor(Plugin plugin)
    {
        if (!await Editor.Show(plugin, _plugins)) return;
        GridSplitter.Width = new GridLength(4);
        EditorColumn.Width = new GridLength(_editorWidth);
    }
        
    private void HideEditor()
    {
        Editor.Hide();
        GridSplitter.Width = new GridLength(0);
        EditorColumn.Width = new GridLength(0);
    }
    
    private async void BtnAddOnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        if (PluginRegister.Plugins.Count == 0)
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
            Plugins = _plugins
        };
        editor.Saved += EditorOnSaved;
        
        await Theme.MessageBox
            .Show("Add plugin", editor, MessageBoxButton.OKCancel, MessageBoxImage.None, editor.Apply);
    }
    
    private void UpdateProject(string? add, string? del)
    {
        DteSingleThread.Run(tcSysManager =>
        {
            tcSysManager.SaveProject();
            if (tcSysManager.GetPlcProject() is not { } plcProjectItem)
            {
                Sdk.Logger.LogError(this, "No Plc project found");
                return;
            }
            if (del is not null) Generator.Generators.Sil.Update(plcProjectItem, del, true);
            if (add is not null) Generator.Generators.Sil.Update(plcProjectItem, add, false);
        });
    }

    private double _editorWidth = 340;

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

        _editorWidth = EditorColumn.Width.Value;
    }
}