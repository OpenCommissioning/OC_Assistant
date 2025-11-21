using System.Windows.Controls;
using OC.Assistant.Common;

namespace OC.Assistant.Plugins;

public partial class ClientDropdown
{
    public Type? SelectedType { get; private set; }
    
    public ClientDropdown()
    {
        InitializeComponent();
        
        Loaded += (_, _) =>
        {
            if (Items.Count > 0) return; 
            Items.Clear();
            
            //var unknown = new ComboBoxItem {Content = "None"};
            //unknown.Selected += ComboBoxItem_Selected;
            //Items.Add(unknown);
            
            foreach (var type in PluginRegister.ClientTypes)
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

    public void SelectType(Type? type)
    {
        foreach (ComboBoxItem item in Items)
        {
            var itemType = item.Tag as Type;
            if (type != itemType) continue;
            SelectedItem = item;
            return;
        }
        
        SelectedIndex = -1;
    }
    
    private void ComboBoxItem_Selected(object sender, EventArgs e)
    {
        var comboBoxItem = (ComboBoxItem)sender;
        SelectedType = comboBoxItem.Tag as Type;
        TypeSelected?.Invoke(SelectedType);
    }
        
    public event Action<Type?>? TypeSelected;
}