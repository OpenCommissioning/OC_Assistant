﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Plugins;

internal partial class Plugin
{
    public Type? Type { get; private set; }
    public IPluginController? PluginController { get; private set; }
    public bool IsValid => PluginController is not null;

    public Plugin()
    {
        InitializeComponent();
        DockPanel.SetDock(this, Dock.Top);
        Name = "MyPlugin";
    }
        
    public Plugin(string name, Type type, XContainer parameter) : this()
    {
        if (!InitType(type)) return;
        PluginController?.Parameter.Update(parameter);
        Name = name;
        BtnEditText.Text = $"{Name}  ({Type?.Name})";
        PluginController?.Initialize(name);
    }

    public bool Save(string name)
    {
        if (!IsValid) return false;
        Name = name;
        PluginController?.Stop();
        BtnEditText.Text = $"{Name}  ({Type?.Name})";
        return PluginController?.Save(name) == true;
    }
        
    public bool InitType(Type? type)
    {
        try
        {
            if (type is null)
            {
                throw new Exception($"{type} is null");
            }
            PluginController?.Stop();
            Type = type;
            PluginController = Activator.CreateInstance(type) as IPluginController;
            if (PluginController is null) return false;
            PluginController.Started += PluginOnStarted;
            PluginController.Stopped += PluginOnStopped;
            PluginController.Starting += PluginOnStarting;
            PluginController.Stopping += PluginOnStopping;
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return false;
        }
    }

    private void PluginOnStopped()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = true;
            StartStopButtonText.Text = "\xE768";
            StartStopButtonText.Foreground = Application.Current.Resources["GreenBrush"] as SolidColorBrush;
        });
    }
    
    private void PluginOnStopping()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = false;
            StartStopButtonText.Text = "\xE71A";
            StartStopButtonText.Foreground = Application.Current.Resources["TextControlForegroundDisabled"] as SolidColorBrush;
        });
    }

    private void PluginOnStarted()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = true;
            StartStopButtonText.Text = "\xE71A";
            StartStopButtonText.Foreground = Application.Current.Resources["DangerBrush"] as SolidColorBrush;
        });
    }
    
    private void PluginOnStarting()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = false;
            StartStopButtonText.Text = "\xE768";
            StartStopButtonText.Foreground = Application.Current.Resources["TextControlForegroundDisabled"] as SolidColorBrush;
        });
    }

    public void Start()
    {
        if (PluginController?.IsRunning == true) return;
        PluginController?.Start();
    }
        
    public void Stop()
    {
        if (PluginController?.IsRunning != true) return;
        PluginController?.Stop();
    }

    private void StartStopButton_Click(object sender, RoutedEventArgs e)
    {
        if (PluginController?.IsRunning == true)
        {
            Stop();
            return;
        }

        Start();
    }
        
    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        OnEdit?.Invoke(this);
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs name)
    {
        if (PluginController?.IsRunning == true) return;
        if (Theme.MessageBox.Show(
                "Delete?",
                Name, MessageBoxButton.OKCancel, MessageBoxImage.Warning)
            == MessageBoxResult.OK)
        {
            OnRemove?.Invoke(this);
        }
    }

    public new bool IsEnabled
    {
        set
        {
            Dispatcher.Invoke(() =>
            {
                RemoveButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            });
        }
    }
        
    public bool IsSelected
    {
        set => EditButton.Tag = value ? "Selected" : null;
    }

    public event Action<Plugin>? OnRemove;
    public event Action<Plugin>? OnEdit;
}