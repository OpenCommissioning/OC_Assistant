﻿using System.Windows;
using OC.Assistant.Core;
using OC.Assistant.Sdk.Plugin;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OC.Assistant.Plugins;

public partial class PluginManager
{
    private ObservableCollection<Plugin> Plugins { get; } = [];
    
    public PluginManager()
    {
        InitializeComponent();
        ItemsControl.ItemsSource = Plugins;
        Plugins.CollectionChanged += PluginsOnCollectionChanged;
        XmlFile.Instance.Reloaded += XmlOnReloaded;
        ProjectState.Events.Disconnected += OnDisconnect;
        ProjectState.Events.StartedRunning += OnStartedRunning;
        ProjectState.Events.StoppedRunning += OnStoppedRunning;
        ProjectState.Events.Locked += OnLocked;
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

    private void OnLocked(bool value)
    {
        BtnAdd.Visibility = value ? Visibility.Hidden : Visibility.Visible;
        Editor.IsEnabled = !value;
        foreach (var plugin in Plugins)
        {
            plugin.IsEnabled = !value;
        }
    }
    
    private void XmlOnReloaded() => Dispatcher.Invoke(Initialize);


    private void Initialize()
    {
        OnDisconnect();
        XmlFile.Instance.LoadPlugins().ForEach(Plugins.Add);
        BtnAdd.Visibility = Visibility.Visible;
    }

    private void OnDisconnect()
    {
        Plugins.ToList().ForEach(x => Plugins.Remove(x));
        BtnAdd.Visibility = Visibility.Hidden;
        HideEditor();
    }

    private void OnStoppedRunning()
    {
        if (BusyState.IsSet) return;
        Task.Run(async () =>
        {
            foreach (var plugin in Plugins)
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
            foreach (var plugin in Plugins.Where(x => x.PluginController?.AutoStart == true))
            {
                plugin.Start();
                await Task.Delay(plugin.PluginController?.DelayAfterStart ?? 0);
            }
        });
    }
        
    private void RegisterPlugin(Plugin plugin)
    {
        plugin.OnRemove += PluginOnRemove;
        plugin.OnEdit += ShowEditor;
    }
        
    private void UnregisterPlugin(Plugin plugin)
    {
        plugin.PluginController?.Stop();
        plugin.OnRemove -= PluginOnRemove;
        plugin.OnEdit -= ShowEditor;
    }
        
    private void PluginOnRemove(Plugin plugin)
    {
        XmlFile.Instance.RemovePlugin(plugin.Name);
        Plugins.Remove(plugin);
        HideEditor();
        if (plugin.PluginController?.IoType == IoType.None) return;
        UpdateProject(null, plugin.Name);
    }

    private void EditorOnSaved(Plugin plugin, string? oldName)
    {
        XmlFile.Instance.UpdatePlugin(plugin, oldName);

        if (Plugins.FirstOrDefault(x => x == plugin) is null)
        {
            Plugins.Add(plugin);
            oldName = null;
        }
        
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged && oldName is null) return;
        plugin.PluginController?.Stop();
        UpdateProject(plugin.Name, oldName);
        if (Editor.Visibility == Visibility.Visible) ShowEditor(plugin);
    }
    
    private async void ShowEditor(Plugin plugin)
    {
        if (!await Editor.Show(plugin, Plugins)) return;
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
            Plugins = Plugins
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