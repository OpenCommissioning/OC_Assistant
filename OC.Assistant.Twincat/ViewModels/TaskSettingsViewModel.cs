using CommunityToolkit.Mvvm.ComponentModel;
using OC.Assistant.Twincat.Automation;

namespace OC.Assistant.Twincat.ViewModels;

public partial class TaskSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string TaskName { get; set; }
    
    [ObservableProperty] 
    public partial string Filter { get; set; }
    
    [ObservableProperty]
    public partial string ToolTip { get; set; }

    public TaskSettingsViewModel()
    {
        TaskName = TaskGenerator.TaskName;
        Filter = TaskGenerator.Filter;
        ToolTip = 
            """
            Supported patterns
            
             foo   : contains
             foo*  : starts with
             *foo  : ends with
            
            Multiple filters can be combined with commas
            """;
    }
}