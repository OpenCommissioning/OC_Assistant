using System.Windows.Controls;
using OC.Assistant.Core;

namespace OC.Assistant.Generator;

public partial class SettingsTaskDropdown
{
    public string Selected { get; private set; } = XmlFile.Instance.PlcTaskName;
    
    public SettingsTaskDropdown()
    {
        InitializeComponent();
        
        Items.Add(new ComboBoxItem {Content = XmlFile.Instance.PlcTaskName});
        SelectedIndex = 0;
        DropDownOpened += OnOpened;
        return;

        void OnOpened(object? sender, EventArgs e)
        {
            var tasks = new List<string>();
            DteSingleThread.Run(tcSysManager =>
            {
                tasks.AddRange(tcSysManager
                    .GetItem(TcShortcut.NODE_RT_TASKS)
                    .GetChildren()
                    .Where(item => item.ItemSubType == (int)TcSmTreeItemSubType.TaskWithImage)
                    .Select(item => item.Name));
            }, 1000);
            
            Items.Clear();
            foreach (var task in tasks)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = task
                };
                
                comboBoxItem.Selected += ComboBoxItem_Selected;
                Items.Add(comboBoxItem);
            }

            if (Items.Count == 0)
            {
                Items.Add(new ComboBoxItem {Content = "No task found", IsEnabled = false});
                Selected = "";
                return;
            }
            
            foreach(var item in Items.Cast<ComboBoxItem>())
            {
                if (item.Content as string != Selected) continue;
                SelectedIndex = Items.IndexOf(item);
                return;
            }
        }
    }
    
    private void ComboBoxItem_Selected(object sender, EventArgs e)
    {
        var comboBoxItem = (ComboBoxItem)sender;
        Selected = (string)comboBoxItem.Content;
    }
}