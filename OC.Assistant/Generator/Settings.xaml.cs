using System.Windows.Controls;

namespace OC.Assistant.Generator;

public partial class Settings
{
    public Settings()
    {
        InitializeComponent();
        if (Core.XmlFile.Instance.Path is null) return;
        ((ComboBoxItem)PlcDropdown.SelectedItem).Content = Core.XmlFile.Instance.PlcProjectName;
        ((ComboBoxItem)TaskDropdown.SelectedItem).Content = Core.XmlFile.Instance.PlcTaskName;
    }

    public void Save()
    {
        if (Core.XmlFile.Instance.Path is null) return;
        Dispatcher.Invoke(() =>
        {
            Core.XmlFile.Instance.PlcProjectName = PlcDropdown.Selected;
            Core.XmlFile.Instance.PlcTaskName = TaskDropdown.Selected;
        });
    }
}