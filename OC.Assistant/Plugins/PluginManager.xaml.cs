using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

public partial class PluginManager
{
    private List<Plugin> _plugins = [];
    
    public PluginManager()
    {
        Loaded += (_, _) => PluginRegister.Initialize();
        InitializeComponent();
    }

    /// <summary>
    /// Override <see cref="Core.ControlBase.IsLocked"/> to not disable the start/stop button for each plugin.
    /// </summary>
    public override bool IsLocked
    {
        set
        {
            var isEnabled = !value && !ReadOnly;
            BtnAdd.Visibility = isEnabled ? Visibility.Visible : Visibility.Hidden;
            Editor.IsEnabled = isEnabled;
            foreach (var plugin in _plugins)
            {
                plugin.IsEnabled = isEnabled;
            }
        }
    }
    
    private void XmlOnReloaded()
    {
        Dispatcher.Invoke(OnConnect);
    }
    
    public override void OnConnect()
    {
        OnDisconnect();
        Core.XmlFile.Instance.Reloaded += XmlOnReloaded;
        ControlPanel.Children.Remove(BtnAdd);
        _plugins = XmlFile.LoadPlugins();

        foreach (var plugin in _plugins)
        {
            AddPlugin(plugin);
        }
            
        ControlPanel.Children.Add(BtnAdd);
        ScrollView.ScrollToEnd();
        BtnAdd.Visibility = ReadOnly ? Visibility.Hidden : Visibility.Visible;
    }
    
    public override void OnDisconnect()
    {
        Core.XmlFile.Instance.Reloaded -= XmlOnReloaded;
        
        foreach (var plugin in _plugins)
        {
            RemovePlugin(plugin);
        }
        
        BtnAdd.Visibility = Visibility.Hidden;
        HideEditor();
        _plugins.Clear();
    }

    public override void OnTcStopped()
    {
        if (IsBusy) return;
        Task.Run(async () =>
        {
            foreach (var plugin in _plugins)
            {
                plugin.Stop();
                await Task.Delay(100);
            }
        });
    }
        
    public override void OnTcStarted()
    {
        if (IsBusy) return;
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
        if (IsBusy) return;
            
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
        IsBusy = true;
        RemovePlugin(plugin);
        XmlFile.UpdatePlugin(plugin, true);
        _plugins.Remove(plugin);
        IsBusy = false;
        BtnAdd_Click(this, null);
        if (plugin.PluginController?.IoType == IoType.None) return;
        Sdk.ApiLocal.Interface.UpdateSil(plugin.Name, true);
    }

    private void Editor_OnConfirm(Plugin plugin)
    {
        IsBusy = true;
        ControlPanel.Children.Remove(BtnAdd);
            
        XmlFile.UpdatePlugin(plugin);

        if (_plugins.FirstOrDefault(x => x.Name == plugin.Name) == default)
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
        IsBusy = false;
        Plugin_OnEdit(plugin);
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged) return;
        Sdk.ApiLocal.Interface.UpdateSil(plugin.Name, false);
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
}