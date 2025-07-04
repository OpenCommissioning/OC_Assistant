using System.ComponentModel;
using System.Windows;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Theme;

namespace OC.Assistant;

public partial class MainWindow
{
    private static event Action<double>? BlurChanged;
    
    public MainWindow()
    {
        InitializeComponent();
        ReadSettings();

        BusyState.Changed += BusyOverlay.SetState;
        LogViewer.LogFilePath = AppData.LogFilePath;
        Logger.Info += (sender, message) => LogViewer.Add(sender, message, MessageType.Info);
        Logger.Warning += (sender, message) => LogViewer.Add(sender, message, MessageType.Warning);
        Logger.Error += (sender, message) => LogViewer.Add(sender, message, MessageType.Error);

        BlurChanged += radius =>
        {
            Dispatcher.Invoke(() =>
            {
                BlurEffect.Radius = radius;
            });
        };
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
    
    private void MainWindowOnClosing(object sender, CancelEventArgs e)
    {
        WriteSettings();
    }
    
    public static MessageBoxResult ShowMessageBox(string caption, UIElement content, MessageBoxButton button, MessageBoxImage image)
    {
        BlurChanged?.Invoke(10);
        var result = Theme.MessageBox.Show(caption, content, button, image);
        BlurChanged?.Invoke(0);
        return result;
    }
    
    public static MessageBoxResult ShowMessageBox(string caption, string text, MessageBoxButton button, MessageBoxImage image)
    {
        BlurChanged?.Invoke(10);
        var result = Theme.MessageBox.Show(caption, text, button, image);
        BlurChanged?.Invoke(0);
        return result;
    }
}