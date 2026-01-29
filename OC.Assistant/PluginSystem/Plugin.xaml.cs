using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.PluginSystem;

internal partial class Plugin : IPlugin, IDisposable
{
    public Type? ChannelType { get; set; }
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
        
    public Plugin(string name, Type type, Type? channelType, XContainer parameter) : this()
    {
        if (!InitType(type)) return;
        ChannelType = channelType;
        PluginController?.Parameter.Update(parameter);
        Name = name;
        NameText.Text = Name;
        TypeText.Text = Type?.Name;
        ClientTypeText.Text = ChannelType?.Name ?? "<unknown>";
        PluginController?.Initialize(name);
    }

    public void Update(XContainer parameter)
    {
        PluginController?.Parameter.Update(parameter);
    }

    public async Task<bool> SaveAsync(string name)
    {
        if (!IsValid) return false;
        Name = name;
        PluginController.Stop();
        NameText.Text = Name;
        TypeText.Text = Type?.Name;
        ClientTypeText.Text = ChannelType?.Name ?? "<unknown>";
        return await Task.Run(() => PluginController.Save(name));
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

            if (PluginController is not null)
            {
                PluginController.Stop();
                PluginController.Started -= PluginOnStarted;
                PluginController.Stopped -= PluginOnStopped;
                PluginController.Starting -= PluginOnStarting;
                PluginController.Stopping -= PluginOnStopping;
                PluginController.ChannelRequested -= PluginOnChannelRequested;
                PluginController.Dispose();
            }
            
            Type = type;
            PluginController = Activator.CreateInstance(type) as IPluginController;
            if (PluginController is null) return false;
            PluginController.Started += PluginOnStarted;
            PluginController.Stopped += PluginOnStopped;
            PluginController.Starting += PluginOnStarting;
            PluginController.Stopping += PluginOnStopping;
            PluginController.ChannelRequested += PluginOnChannelRequested;
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return false;
        }
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
    
    private ChannelBase? PluginOnChannelRequested()
    {
        if (PluginController is null || ChannelType is null) return null;
        
        var readSize = PluginController.IoType == IoType.Struct ? 
            PluginController.InputStructure.Length : 
            PluginController.InputAddress.Length;
        
        var writeSize = PluginController.IoType == IoType.Struct ? 
            PluginController.OutputStructure.Length : 
            PluginController.OutputAddress.Length;

        try
        {
            return Activator.CreateInstance(ChannelType, writeSize, readSize) as ChannelBase;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return null;
        }
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
    
    private void PluginOnStopping()
    {
        Dispatcher.Invoke(() =>
        {
            StartStopButton.IsEnabled = false;
            StopIcon.Foreground = Application.Current.Resources["White3Brush"] as SolidColorBrush;
            StartIcon.Foreground = Application.Current.Resources["TransparentBrush"] as SolidColorBrush;
        });
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

    public void Start()
    {
        if (ChannelType is null) Logger.LogWarning(this, $"No channel for {Name} selected");
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

    public bool IsSelected
    {
        get;
        set
        {
            field = value;
            EditButton.Tag = value ? "Selected" : null;
        }
    }

    public event Action<Plugin>? OnRemove;
    public event Action<Plugin>? OnEdit;

    public void Dispose()
    {
        PluginController?.Stop();
        PluginController?.Dispose();
    }
}