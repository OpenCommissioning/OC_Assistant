using System.Windows;
using System.Windows.Controls;
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
            XmlFile.Instance.Reload();
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
            XmlFile.Instance.Reload();
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
    
    private void CreateTemplateOnClick(object sender, RoutedEventArgs e)
    {
        var input = new TextBox { Height = 24, Text = "DeviceName" };

        if (Theme.MessageBox
                .Show("Create device template", input, MessageBoxButton.OKCancel, MessageBoxImage.None) !=
            MessageBoxResult.OK)
        {
            return;
        }

        var name = input.Text;
        
        DteSingleThread.Run(dte =>
        {
            if (GetPlcProject(dte) is not {} plcProjectItem) return;
            if (plcProjectItem.GetOrCreateChild("_generated_templates_", 
                    TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} folder) return;
            Generators.DeviceTemplate.Create(folder, name);
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
        XmlFile.Instance.ClientUpdate(config);
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