using System.Diagnostics.CodeAnalysis;
using System.Windows;
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
    
    [MemberNotNullWhen(true, nameof(Type))]
    [MemberNotNullWhen(true, nameof(PluginController))]
    public bool IsValid => Type is not null && PluginController is not null;

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
        PluginController.Stop();
        BtnEditText.Text = $"{Name}  ({Type.Name})";
        return PluginController.Save(name);
    }
        
    [MemberNotNullWhen(true, nameof(Type))]
    [MemberNotNullWhen(true, nameof(PluginController))]
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
            StartIcon.Foreground = Application.Current.Resources["SuccessBrush"] as SolidColorBrush;
            StopIcon.Foreground = Application.Current.Resources["TransparentBrush"] as SolidColorBrush;
        });
    }
    
    private void PluginOnStopping()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = false;
            StopIcon.Foreground = Application.Current.Resources["White3Brush"] as SolidColorBrush;
            StartIcon.Foreground = Application.Current.Resources["TransparentBrush"] as SolidColorBrush;
        });
    }

    private void PluginOnStarted()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = true;
            StartIcon.Foreground = Application.Current.Resources["TransparentBrush"] as SolidColorBrush;
            StopIcon.Foreground = Application.Current.Resources["DangerBrush"] as SolidColorBrush;
        });
    }
    
    private void PluginOnStarting()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = false;
            StartIcon.Foreground = Application.Current.Resources["White3Brush"] as SolidColorBrush;
            StopIcon.Foreground = Application.Current.Resources["TransparentBrush"] as SolidColorBrush;
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

    private async void RemoveButton_Click(object sender, RoutedEventArgs name)
    {
        try
        {
            if (PluginController?.IsRunning == true) return;
            if (await Theme.MessageBox.Show(
                    "Plugins",
                    $"Delete {Name}?", MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                == MessageBoxResult.OK)
            {
                OnRemove?.Invoke(this);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
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