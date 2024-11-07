using System.Windows.Controls;
using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Controls;

/// <summary>
/// Dropdown for available solutions.
/// </summary>
public partial class DteSelector
{
    public event Action<TcDte>? Selected;

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
                Header = instance.SolutionFullName,
                Tag = instance
            };
            
            subMenuItem.Click += (obj, _) =>
            {
                if (((MenuItem)obj).Tag is TcDte dte)
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