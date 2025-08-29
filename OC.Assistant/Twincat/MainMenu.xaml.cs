using System.Windows;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Plugins;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat;

public partial class MainMenu
{
    private static bool IsSolutionConnected => TcState.Instance.SolutionFullName is not null;
    
    public MainMenu()
    {
        Visibility = Visibility.Collapsed;
        InitializeComponent();
        TcState.Instance.Validated += TcStateOnValidated;
    }

    private void TcStateOnValidated()
    {
        Visibility = Visibility.Visible;
        ProjectState.Events.Locked += ProjectStateOnLocked;
        WebApi.ConfigReceived += ApiOnConfigReceived;
        PluginManager.PluginUpdated += PluginManagerOnPluginUpdate;
        Controls.WelcomePage.AddContent(new WelcomePage());
    }

    private void ProjectStateOnLocked(bool isLocked) => IsEnabled = !isLocked && IsSolutionConnected;

    private void CreateProjectOnClick(object sender, RoutedEventArgs e)
    {
        if (!IsSolutionConnected) return;
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
        if (!IsSolutionConnected) return;
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
        if (!IsSolutionConnected) return;
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
            if (!IsSolutionConnected) return;
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
            if (!IsSolutionConnected) return;
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
    
    private void PluginManagerOnPluginUpdate(string? add, string? del)
    {
        if (!IsSolutionConnected) return;
        DteSingleThread.Run(tcSysManager =>
        {
            tcSysManager.SaveProject();
            if (tcSysManager.GetPlcProject() is not { } plcProjectItem)
            {
                Logger.LogError(this, "No Plc project found");
                return;
            }
            if (del is not null) Generators.Sil.Update(plcProjectItem, del, true);
            if (add is not null) Generators.Sil.Update(plcProjectItem, add, false);
        });
    }
    
    private void ApiOnConfigReceived(XElement config)
    {
        XmlFile.Instance.Main = config;
        XmlFile.Instance.Save();
        
        DteSingleThread.Run(tcSysManager =>
        {
            if (GetPlcProject(tcSysManager) is not { } plcProjectItem)
            {
                throw new Exception("No Plc project found");
            }
            Generators.Project.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        }, throwExceptions: true);
    }
    
    private ITcSmTreeItem? GetPlcProject(ITcSysManager15? tcSysManager)
    {
        tcSysManager?.SaveProject();
        if (tcSysManager?.GetPlcProject() is {} plcProjectItem) return plcProjectItem;
        Logger.LogError(this, "No Plc project found");
        return null;
    }
}