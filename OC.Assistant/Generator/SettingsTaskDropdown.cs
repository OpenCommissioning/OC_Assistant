using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;

namespace OC.Assistant.Generator;

/// <summary>
/// Dropdown for available tasks.
/// </summary>
public class SettingsTaskDropdown : ComboBox
{
    public string Selected { get; private set; } = XmlFile.Instance.PlcTaskName;
    
    public SettingsTaskDropdown()
    {
        Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;
        Items.Add(new ComboBoxItem {Content = XmlFile.Instance.PlcTaskName});
        SelectedIndex = 0;
        DropDownOpened += OnOpened;
        return;

        void OnOpened(object? sender, EventArgs e)
        {
            var tasks = new List<string>();
            DteSingleThread.Run(dte =>
            {
                if (dte.GetTcSysManager() is not {} tcSysManager) return;
                
                tasks.AddRange(tcSysManager
                    .TryGetItems(TcShortcut.TASK)
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