using System.Windows.Controls;
using EnvDTE;
using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Controls;

/// <summary>
/// Dropdown for available solutions.
/// </summary>
public partial class DteSelector
{
    public event Action<DTE>? Selected;

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
                Header = instance.GetSolutionFullName(),
                Tag = instance
            };
            
            subMenuItem.Click += (obj, _) =>
            {
                if (((MenuItem)obj).Tag is DTE dte)
                {
                    Selected?.Invoke(dte);
                }
            };
            
            Items.Add(subMenuItem);
        }

        if (Items.Count == 0)
        {
            Items.Add(new MenuItem {Header = "no open TwinCAT solution", IsEnabled = false});
        }
    }
}