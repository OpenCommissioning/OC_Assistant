using System.ComponentModel;
using System.Windows;
using OC.Assistant.Api;
using OC.Assistant.Common;
using OC.Assistant.PluginSystem;
using OC.Assistant.Sdk;
using OC.Assistant.Theme;

namespace OC.Assistant;

public partial class MainWindow
{
    public MainWindow()
    {
        Loaded += OnLoaded;
        var messages = new Queue<(object, string, MessageType)>();
        LogFileWriter.Create();
        Logger.Info += LoggerOnInfo;
        Logger.Warning += LoggerOnWarning;
        Logger.Error += LoggerOnError;
        
        InitializeComponent();
        
        Logger.Info -= LoggerOnInfo;
        Logger.Warning -= LoggerOnWarning;
        Logger.Error -= LoggerOnError;
        
        Logger.Info += (sender, message) => LogViewer.Add(sender, message, MessageType.Info);
        Logger.Warning += (sender, message) => LogViewer.Add(sender, message, MessageType.Warning);
        Logger.Error += (sender, message) => LogViewer.Add(sender, message, MessageType.Error);
        
        while (messages.TryDequeue(out var m))
        {
            LogViewer.Add(m.Item1, m.Item2, m.Item3);
        }
        
        SetSizeAndPosition();
        
        WebApi.RunDetached();
        NamedPipeApi.RunDetached();
        TcpIpServer.RunDetached();
        return;

        void LoggerOnInfo(object sender, string message) => messages.Enqueue((sender, message, MessageType.Info));
        void LoggerOnWarning(object sender, string message) => messages.Enqueue((sender, message, MessageType.Warning));
        void LoggerOnError(object sender, string message) => messages.Enqueue((sender, message, MessageType.Error));
    }

    private static void OnLoaded(object sender, RoutedEventArgs e) => BusyState.Changed += BusyOverlay.SetState;

    private void SetSizeAndPosition()
    {
        var settings = AppControl.Instance.Settings;
        Height = settings.Height < 100 ? 400 : settings.Height;
        Width = settings.Width < 150 ? 600 : settings.Width;
        Left = settings.PosX;
        Top = settings.PosY;
        ConsoleRow.Height = new GridLength(settings.ConsoleHeight);
    }

    private void WriteSettings()
    {
        if (WindowState == WindowState.Maximized) return;
        var settings = AppControl.Instance.Settings;
        settings.Height = (int) Height;
        settings.Width = (int) Width;
        settings.PosX = (int) Left;
        settings.PosY = (int) Top;
        settings.ConsoleHeight = (int) ConsoleRow.Height.Value;
        settings.Write();
    }

    private async void MainWindowOnClosing(object sender, CancelEventArgs e)
    {
        try
        {
            await NamedPipeApi.CloseAsync();
            await TcpIpServer.CloseAsync();
            WriteSettings();
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }

    private void LogViewerOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var maxHeight = Height - 60;
        if (ConsoleRow.Height.Value > maxHeight)
        {
            ConsoleRow.Height = new GridLength(maxHeight);
        }
    }
}