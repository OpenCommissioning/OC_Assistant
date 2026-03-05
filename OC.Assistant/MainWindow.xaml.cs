using System.ComponentModel;
using System.Windows;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Theme;

namespace OC.Assistant;

public partial class MainWindow
{
    public MainWindow()
    {
        var messages = new Queue<(object, string, MessageType)>();
        LogFileWriter.Create();
        BusyState.Changed += BusyOverlay.ChangeState;
        Logger.Info += LoggerOnInfo;
        Logger.Warning += LoggerOnWarning;
        Logger.Error += LoggerOnError;
        
        InitializeComponent();
        ReadSettings();
        
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
        
        return;

        void LoggerOnInfo(object sender, string message) => messages.Enqueue((sender, message, MessageType.Info));
        void LoggerOnWarning(object sender, string message) => messages.Enqueue((sender, message, MessageType.Warning));
        void LoggerOnError(object sender, string message) => messages.Enqueue((sender, message, MessageType.Error));
    }

    private void ReadSettings()
    {
        var settings = new Settings().Read();
        Height = settings.Height < 100 ? 400 : settings.Height;
        Width = settings.Width < 150 ? 600 : settings.Width;
        Left = settings.PosX;
        Top = settings.PosY;
        ConsoleRow.Height = new GridLength(settings.ConsoleHeight);
    }

    private void WriteSettings()
    {
        if (WindowState == WindowState.Maximized) return;
        
        new Settings 
        {
            Height = (int) Height, 
            Width = (int) Width, 
            PosX = (int) Left, 
            PosY = (int) Top, 
            ConsoleHeight = (int) ConsoleRow.Height.Value
        }.Write();
    }
    
    private void MainWindowOnClosing(object sender, CancelEventArgs e) => WriteSettings();

    private void LogViewerOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var maxHeight = Height - 60;
        if (ConsoleRow.Height.Value > maxHeight)
        {
            ConsoleRow.Height = new GridLength(maxHeight);
        }
    }
}