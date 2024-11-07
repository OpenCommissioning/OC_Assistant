namespace OC.Assistant.Generator;

public partial class Settings
{
    public Settings()
    {
        InitializeComponent();

        if (Core.XmlFile.Path is null) return;
        PlcProjectName.Text = Core.XmlFile.Instance.PlcProjectName;
        PlcTaskName.Text = Core.XmlFile.Instance.PlcTaskName;
        TaskAutoUpdate.IsChecked = Core.XmlFile.Instance.TaskAutoUpdate;
    }

    public void Save()
    {
        if (Core.XmlFile.Path is null) return;
        Dispatcher.Invoke(() =>
        {
            Core.XmlFile.Instance.PlcProjectName = PlcProjectName.Text;
            Core.XmlFile.Instance.PlcTaskName = PlcTaskName.Text;
            Core.XmlFile.Instance.TaskAutoUpdate = TaskAutoUpdate.IsChecked == true;
        });
    }
}