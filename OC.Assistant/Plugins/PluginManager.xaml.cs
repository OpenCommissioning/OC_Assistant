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
        ControlPanel.Children.Remove(BtnAdd);
        _plugins = XmlFile.Instance.LoadPlugins();

        foreach (var plugin in _plugins)
        {
            AddPlugin(plugin);
        }
            
        ControlPanel.Children.Add(BtnAdd);
        ScrollView.ScrollToEnd();
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
            ControlPanel.Children.Add(plugin);
            DockPanel.SetDock(plugin, Dock.Top);
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
        BusyState.Set(this);
        RemovePlugin(plugin);
        XmlFile.Instance.RemovePlugin(plugin.Name);
        _plugins.Remove(plugin);
        HideEditor();
        BusyState.Reset(this);
        if (plugin.PluginController?.IoType == IoType.None) return;
        UpdateProject(plugin.Name, true);
    }

    private void EditorOnSaved(Plugin plugin)
    {
        BusyState.Set(this);
        ControlPanel.Children.Remove(BtnAdd);
            
        XmlFile.Instance.UpdatePlugin(plugin);

        if (_plugins.FirstOrDefault(x => x.Name == plugin.Name) is null)
        {
            _plugins.Add(plugin);
            AddPlugin(plugin);
        }
        else
        {
            plugin.PluginController?.Stop();
        }
            
        ControlPanel.Children.Add(BtnAdd);
        ScrollView.ScrollToEnd();
        BusyState.Reset(this);
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged) return;
        UpdateProject(plugin.Name, false);
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
    
    private void UpdateProject(string name, bool delete)
    {
        DteSingleThread.Run(tcSysManager =>
        {
            tcSysManager.SaveProject();
            if (tcSysManager.GetPlcProject() is not { } plcProjectItem)
            {
                Sdk.Logger.LogError(this, "No Plc project found");
                return;
            }
            Generator.Generators.Sil.Update(plcProjectItem, name, delete);
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