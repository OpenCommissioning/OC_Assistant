using System.Windows;
using System.Windows.Controls;

namespace OC.Assistant.Plugins;

/// <summary>
/// Dropdown for available plugins.
/// </summary>
public class PluginDropdown : ComboBox
{
    public Type? SelectedType { get; private set; }

    public PluginDropdown()
    {
        Style = Application.Current.Resources["DefaultComboBoxStyle"] as Style;
        Loaded += (_, _) =>
        {
            Items.Clear();
            foreach (var type in PluginRegister.Types)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = type.Name,
                    Tag = type
                };
                
                comboBoxItem.Selected += ComboBoxItem_Selected;
                Items.Add(comboBoxItem);
            }
            SelectedIndex = 0;
        };
    }

    private void ComboBoxItem_Selected(object sender, EventArgs e)
    {
        var comboBoxItem = (ComboBoxItem)sender;
        SelectedType = (Type)comboBoxItem.Tag;
        TypeSelected?.Invoke(SelectedType);
    }
        
    public event Action<Type>? TypeSelected;
}