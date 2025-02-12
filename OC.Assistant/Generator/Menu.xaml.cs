﻿using System.Windows;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using EnvDTE;
using TCatSysManagerLib;

namespace OC.Assistant.Generator;

public partial class Menu
{
    public Menu()
    {
        InitializeComponent();
        ProjectState.Events.Locked += isLocked => IsEnabled = !isLocked;
        ApiLocal.Interface.ConfigReceived += ApiOnConfigReceived;
    }

    private void CreateProjectOnClick(object sender, RoutedEventArgs e)
    {
        DteSingleThread.Run(dte =>
        {
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            Core.XmlFile.Instance.Reload();
            Generators.Hil.Update(dte, plcProjectItem);
            Generators.Project.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private void CreatePluginsOnClick(object sender, RoutedEventArgs e)
    {
        DteSingleThread.Run(dte =>
        {
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            Core.XmlFile.Instance.Reload();
            Generators.Sil.UpdateAll(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private void CreateTaskOnClick(object sender, RoutedEventArgs e)
    {
        DteSingleThread.Run(dte =>
        {
            Generators.Task.CreateVariables(dte);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private void SettingsOnClick(object sender, RoutedEventArgs e)
    {
        var settings = new Settings();

        if (Theme.MessageBox
                .Show("Project Settings", settings, MessageBoxButton.OKCancel, MessageBoxImage.None) ==
            MessageBoxResult.OK)
        {
            settings.Save();
        }
    }
    
    private void ApiOnConfigReceived(XElement config)
    {
        XmlFile.ClientUpdate(config);
        DteSingleThread.Run(dte =>
        {
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            Generators.Project.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private ITcSmTreeItem? GetPlcProject(DTE? dte)
    {
        var tcSysManager = dte?.GetTcSysManager();
        tcSysManager?.SaveProject();
        if (tcSysManager?.TryGetPlcProject() is {} plcProjectItem) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        return null;
    }
}