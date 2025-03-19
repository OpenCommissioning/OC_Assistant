using System.Windows;
using System.Windows.Controls;
using OC.Assistant.Core;

namespace OC.Assistant.Generator;

/// <summary>
/// Dropdown for available plc projects.
/// </summary>
public class SettingsPlcDropdown : ComboBox
{
    public string Selected { get; private set; } = Core.XmlFile.Instance.PlcProjectName;
    
    public SettingsPlcDropdown()
    {
        Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;
        Items.Add(new ComboBoxItem {Content = Core.XmlFile.Instance.PlcProjectName});
        SelectedIndex = 0;
        DropDownOpened += OnOpened;
        return;

        void OnOpened(object? sender, EventArgs e)
        {
            var projects = new List<string>();
            DteSingleThread.Run(dte =>
            {
                if (dte.GetTcSysManager() is not {} tcSysManager) return;
                projects.AddRange(tcSysManager
                    .TryGetItems(TcShortcut.PLC)
                    .Select(item => item.Name));
            }, true);
            
            Items.Clear();
            foreach (var project in projects)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = project
                };
                
                comboBoxItem.Selected += ComboBoxItem_Selected;
                Items.Add(comboBoxItem);
            }

            if (Items.Count == 0)
            {
                Items.Add(new ComboBoxItem {Content = "No plc found", IsEnabled = false});
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