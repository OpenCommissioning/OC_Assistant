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
    
    private void PluginManagerOnLoaded(object sender, RoutedEventArgs e)
    {
        PluginRegister.Initialize();
    }
    
    private void OnLocked(bool value)
    {
        var isEnabled = !value;
        BtnAdd.Visibility = isEnabled ? Visibility.Visible : Visibility.Hidden;
        Editor.IsEnabled = isEnabled;
        foreach (var plugin in _plugins)
        {
            plugin.IsEnabled = isEnabled;
        }
    }
    
    private void XmlOnReloaded()
    {
        Dispatcher.Invoke(Initialize);
    }

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
        DeselectAll();
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
        plugin.OnRemove += Plugin_OnRemove;
        plugin.OnEdit += Plugin_OnEdit;

        Dispatcher.Invoke(() =>
        {
            ControlPanel.Children.Add(plugin);
            DockPanel.SetDock(plugin, Dock.Top);
        });
    }
        
    private void RemovePlugin(Plugin plugin)
    {
        plugin.PluginController?.Stop();
        plugin.OnRemove -= Plugin_OnRemove;
        plugin.OnEdit -= Plugin_OnEdit;

        Dispatcher.Invoke(() =>
        {
            ControlPanel.Children.Remove(plugin);
        });
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs? e)
    {
        if (BusyState.IsSet) return;
            
        var plugin = new Plugin();
        if (!Editor.Show(plugin, _plugins)) return;
        DeselectAll();
        BtnAddIsSelected = true;
        ShowEditor();
    }
    
    private void Plugin_OnEdit(Plugin plugin)
    {
        if (!Editor.Show(plugin, _plugins)) return;
        ShowEditor();
        DeselectAll();
        plugin.IsSelected = true;
    }
        
    private void Plugin_OnRemove(Plugin plugin)
    {
        BusyState.Set(this);
        RemovePlugin(plugin);
        XmlFile.Instance.RemovePlugin(plugin);
        _plugins.Remove(plugin);
        BusyState.Reset(this);
        BtnAdd_Click(this, null);
        if (plugin.PluginController?.IoType == IoType.None) return;
        UpdateProject(plugin.Name, true);
    }

    private void Editor_OnConfirm(Plugin plugin)
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
        Plugin_OnEdit(plugin);
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged) return;
        UpdateProject(plugin.Name, false);
    }
        
    private void Editor_OnCancel()
    {
        DeselectAll();
        HideEditor();
    }

    private void ShowEditor()
    {
        GridSplitter.Width = new GridLength(4);
        if (EditorColumn.ActualWidth <= 0) EditorColumn.Width = new GridLength(400);
    }
        
    private void HideEditor()
    {
        GridSplitter.Width = new GridLength(0);
        EditorColumn.Width = new GridLength(0);
        Editor.Visibility = Visibility.Collapsed;
    }
    
    private bool BtnAddIsSelected
    {
        set => BtnAdd.Tag = value ? "Selected" : null;
    }

    private void DeselectAll()
    {
        BtnAddIsSelected = false; 
        foreach (var plugin in _plugins)
        {
            plugin.IsSelected = false;
        }
    }
    
    private void UpdateProject(string name, bool delete)
    {
        DteSingleThread.Run(dte =>
        {
            var tcSysManager = dte.GetTcSysManager();
            tcSysManager?.SaveProject();
            if (tcSysManager?.TryGetPlcProject() is not { } plcProjectItem)
            {
                Sdk.Logger.LogError(this, "No Plc project found");
                return;
            }
            Generator.Generators.Sil.Update(plcProjectItem, name, delete);
        });
    }
}