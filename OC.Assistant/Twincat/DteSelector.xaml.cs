using System.Windows.Controls;
using OC.Assistant.Core;

namespace OC.Assistant.Twincat;

/// <summary>
/// Dropdown for available solutions.
/// </summary>
public partial class DteSelector
{
    private readonly struct Solution
    {
        public string? SolutionFullName { get; init; }
        public string? ProjectFolder { get; init; }
    }
    
    private readonly List<Solution> _solutions = [];
    
    public DteSelector()
    {
        InitializeComponent();
        SubmenuOpened += OnSubmenuOpened;
    }

    private void OnSubmenuOpened(object sender, EventArgs e)
    {
        Items.Clear();
        _solutions.Clear();
        
        DteSingleThread.Run(() =>
        {
            foreach (var dte in TcDte.GetInstances())
            {
                var solution = dte.Solution;
                _solutions.Add(new Solution
                {
                    SolutionFullName = solution?.FullName,
                    ProjectFolder = dte.GetProjectFolder()
                });
                ComHelper.ReleaseObject(solution);
                ComHelper.ReleaseObject(dte);
            }
        
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }, 1000);
        
        
        foreach (var solution in _solutions)
        {
            var subMenuItem = new MenuItem
            {
                Header = solution.SolutionFullName,
                Tag = solution
            };
            
            subMenuItem.Click += (obj, _) =>
            {
                if (((MenuItem) obj).Tag is not Solution tag) return;
                if (string.IsNullOrEmpty(tag.SolutionFullName) || string.IsNullOrEmpty(tag.ProjectFolder)) return;
                ProjectState.Control.Connect(tag.SolutionFullName, tag.ProjectFolder);
            };
                
            Items.Add(subMenuItem);
        }

        if (Items.Count == 0)
        {
            Items.Add(new MenuItem {Header = "no open TwinCAT solution", IsEnabled = false});
        }
    }
}