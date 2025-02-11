using System.Windows.Controls;
using EnvDTE;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

/// <summary>
/// Dropdown for available solutions.
/// </summary>
public partial class DteSelector
{
    public DteSelector()
    {
        InitializeComponent();
        SubmenuOpened += OnSubmenuOpened;
    }

    private void OnSubmenuOpened(object sender, EventArgs e)
    {
        Items.Clear();
        
        foreach (var instance in TcDte.GetInstances())
        {
            var subMenuItem = new MenuItem
            {
                Header = instance.GetSolutionFullName()
            };
            
            instance.Finalize(false);
            
            subMenuItem.Click += (obj, _) =>
            {
                if (((MenuItem)obj).Header is string path)
                {
                    ProjectState.Solution.Connect(path);
                }
            };
            
            Items.Add(subMenuItem);
        }

        if (Items.Count == 0)
        {
            Items.Add(new MenuItem {Header = "no open TwinCAT solution", IsEnabled = false});
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}