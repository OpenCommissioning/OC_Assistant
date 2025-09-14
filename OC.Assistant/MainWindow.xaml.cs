using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Theme;

namespace OC.Assistant;

public partial class MainWindow
{
    private AppSettings AppSettings { get; } = new AppSettings().Read();
    
    public MainWindow()
    {
        InitializeComponent();
        SetSizeAndPosition();
        
        BusyState.Changed += BusyOverlay.SetState;
        LogFileWriter.Create();
        Logger.Info += (sender, message) => LogViewer.Add(sender, message, MessageType.Info);
        Logger.Warning += (sender, message) => LogViewer.Add(sender, message, MessageType.Warning);
        Logger.Error += (sender, message) => LogViewer.Add(sender, message, MessageType.Error);
        WebApi.BuildAndRun(AppSettings);
        
        foreach (var package in PackageRegister.Packages)
        {
            if (Activator.CreateInstance(package.Type, AppControl.Instance) is not MenuItem menu) continue;
            MainMenu.Items.Insert(1, menu);
        }
    }
    
    private void SetSizeAndPosition()
    {
        Height = AppSettings.Height < 100 ? 400 : AppSettings.Height;
        Width = AppSettings.Width < 150 ? 600 : AppSettings.Width;
        Left = AppSettings.PosX;
        Top = AppSettings.PosY;
        ConsoleRow.Height = new GridLength(AppSettings.ConsoleHeight);
    }

    private void WriteSettings()
    {
        if (WindowState == WindowState.Maximized) return;
        AppSettings.Height = (int) Height;
        AppSettings.Width = (int) Width;
        AppSettings.PosX = (int) Left;
        AppSettings.PosY = (int) Top;
        AppSettings.ConsoleHeight = (int) ConsoleRow.Height.Value;
        AppSettings.Write();
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