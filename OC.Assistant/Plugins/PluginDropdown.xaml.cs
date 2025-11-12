using System.Windows.Controls;
using OC.Assistant.Common;

namespace OC.Assistant.Plugins;

public partial class PluginDropdown
{
    public Type? SelectedType { get; private set; }
    
    public PluginDropdown()
    {
        InitializeComponent();
        
        Loaded += (_, _) =>
        {
            Items.Clear();
            foreach (var type in PluginRegister.Plugins.Select(x => x.Type))
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