using System.Windows;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
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
        DteSingleThread.Run(tcSysManager =>
        {
            if (GetPlcProject(tcSysManager) is not {} plcProjectItem) return;
            XmlFile.Instance.Reload();
            Generators.Hil.Update(tcSysManager, plcProjectItem);
            Generators.Project.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private void CreatePluginsOnClick(object sender, RoutedEventArgs e)
    {
        DteSingleThread.Run(dte =>
        {
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            XmlFile.Instance.Reload();
            Generators.Sil.UpdateAll(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private void CreateTaskOnClick(object sender, RoutedEventArgs e)
    {
        DteSingleThread.Run(tcSysManager =>
        {
            Generators.Task.CreateVariables(tcSysManager);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private async void CreateTemplateOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var generator = new Generators.DeviceTemplate();

            if (await Theme.MessageBox.Show(
                    "Create device template", 
                    generator.InputField, 
                    MessageBoxButton.OKCancel, 
                    MessageBoxImage.None,
                    generator.CheckName) !=
                MessageBoxResult.OK)
            {
                return;
            }
            
            generator.Create();
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }
    
    private async void SettingsOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = new Settings();

            if (await Theme.MessageBox.Show("Project Settings", settings, MessageBoxButton.OKCancel, MessageBoxImage.None) ==
                MessageBoxResult.OK)
            {
                settings.Save();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }
    
    private void ApiOnConfigReceived(XElement config)
    {
        XmlFile.Instance.Main = config;
        XmlFile.Instance.Save();
        
        DteSingleThread.Run(tcSysManager =>
        {
            if (GetPlcProject(tcSysManager) is not {} plcProjectItem) return;
            Generators.Project.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        });
    }
    
    private ITcSmTreeItem? GetPlcProject(ITcSysManager15? tcSysManager)
    {
        tcSysManager?.SaveProject();
        if (tcSysManager?.GetPlcProject() is {} plcProjectItem) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        return null;
    }
}