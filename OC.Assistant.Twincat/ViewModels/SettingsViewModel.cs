using CommunityToolkit.Mvvm.ComponentModel;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial List<string> PlcProjects { get; set; } = [];
    
    [ObservableProperty]
    public partial List<string> PlcTasks { get; set; } = [];
    
    [ObservableProperty]
    public partial string? SelectedPlcProject { get; set; }
    
    [ObservableProperty] 
    public partial string? SelectedPlcTask { get; set; }

    public SettingsViewModel()
    {
        SelectedPlcProject = XmlFile.Instance.PlcProjectName;
        SelectedPlcTask = XmlFile.Instance.PlcTaskName;
        
        GetPlcProjects();
        GetTasks();
    }
    
    public bool Save()
    {
        XmlFile.Instance.PlcProjectName = SelectedPlcProject;
        XmlFile.Instance.PlcTaskName = SelectedPlcTask;
        XmlFile.Instance.Save();
        return true;
    }
    
    private void GetPlcProjects()
    {
        PlcProjects.Clear();

        DteSingleThread.Run(tcSysManager =>
        {
            PlcProjects.AddRange(tcSysManager
                .GetItem(TcShortcut.NODE_PLC_CONFIG)
                .GetChildren()
                .Select(item => item.Name));
        }, 1000);
    }
    
    private void GetTasks()
    {
        PlcTasks.Clear();

        DteSingleThread.Run(tcSysManager =>
        {
            PlcTasks.AddRange(tcSysManager
                .GetItem(TcShortcut.NODE_RT_TASKS)
                .GetChildren()
                .Where(item => item.ItemSubType == (int)TcSmTreeItemSubType.TaskWithImage)
                .Select(item => item.Name));
        }, 1000);
    }
}