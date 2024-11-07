﻿using System.ComponentModel;
using System.Windows;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using OC.Assistant.Theme;

namespace OC.Assistant;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ProjectManager.Instance.Initialize(Footer);
        ReadSettings();

        BusyState.Changed += BusyOverlay.SetState;
        LogViewer.LogFilePath = AppData.LogFilePath;
        Logger.Info += (sender, message) => LogViewer.Add(sender, message, MessageType.Info);
        Logger.Warning += (sender, message) => LogViewer.Add(sender, message, MessageType.Warning);
        Logger.Error += (sender, message) => LogViewer.Add(sender, message, MessageType.Error);
        
        _ = WinApi.TimeBeginPeriod(1);
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
            ConsoleHeight = (int) ConsoleRow.Height.Value, 
        }.Write();
    }
    
    private void MainWindowOnClosing(object sender, CancelEventArgs e)
    {
        _ = WinApi.TimeEndPeriod(1);
        WriteSettings();
    }
}