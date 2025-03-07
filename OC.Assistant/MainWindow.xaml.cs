using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using OC.Assistant.Controls;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Theme;

namespace OC.Assistant;

public partial class MainWindow
{
    public static async Task<string?> CompareVersion(string currentVersion, bool message = false)
    {
        try
        {
            const string url = "https://api.github.com/repos/opencommissioning/oc_assistant/releases/latest";
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
            var latestRelease = JsonDocument
                .Parse(await client.GetStringAsync(url))
                .RootElement
                .GetProperty("tag_name")
                .GetString();
            
            if (latestRelease != null && latestRelease != currentVersion)
            {
                //if (messageBox)
                //{
                //    Theme.MessageBox.Show("New version available", new DownloadLink(latestRelease), MessageBoxButton.OK, MessageBoxImage.Information);
                //    return true;
                //}

                if (message)
                {
                    Logger.LogWarning(typeof(MainWindow), $"New Release {latestRelease} available on GitHub");
                }
               
                return latestRelease;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }
    
    public MainWindow()
    {
        InitializeComponent();
        Footer.Children.Add(ProjectState.View);
        ReadSettings();

        BusyState.Changed += BusyOverlay.SetState;
        LogViewer.LogFilePath = AppData.LogFilePath;
        Logger.Info += (sender, message) => LogViewer.Add(sender, message, MessageType.Info);
        Logger.Warning += (sender, message) => LogViewer.Add(sender, message, MessageType.Warning);
        Logger.Error += (sender, message) => LogViewer.Add(sender, message, MessageType.Error);
        
        Loaded += OnLoaded;

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await CompareVersion("v1.5.0");
        }
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
}