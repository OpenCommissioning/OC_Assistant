using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OC.Assistant.Twincat.ViewModels;

public class SolutionItem(string name, string? projectFolder = null)
{
    public string Name => name;
    public bool IsEnabled => projectFolder is not null;
    public RelayCommand? Command => projectFolder is null ? null : new RelayCommand(() =>
    {
        TcState.Singleton.ConnectSolution(Path.Combine(projectFolder, "OC.Assistant.xml"), name);
    });
}

public partial class DteSelectorViewModel : ObservableObject
{
    public List<SolutionItem> Items { get; set; } = [];

    [ObservableProperty]
    public partial bool IsSubmenuOpen { get; set; }
    
    [RelayCommand]
    private void OpenSubmenu()
    {
        Items.Clear();
        
        DteSingleThread.Run(() =>
        {
            foreach (var dte in TcDte.GetInstances())
            {
                var solution = dte.Solution;
                var fullName = solution?.FullName;
                var projectFolder = dte.GetProjectFolder();
                
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(projectFolder)) continue;
                Items.Add(new SolutionItem(fullName, projectFolder));
                
                TcDte.ReleaseObject(solution);
                TcDte.ReleaseObject(dte);
            }
        
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }, 1000);
        
        if (Items.Count == 0) Items.Add(new SolutionItem("No open TwinCAT solution"));
        IsSubmenuOpen =  true;
    }
}