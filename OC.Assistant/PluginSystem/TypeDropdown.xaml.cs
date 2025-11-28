using System.Windows.Controls;

namespace OC.Assistant.PluginSystem;

public partial class TypeDropdown
{
    // ReSharper disable once CollectionNeverUpdated.Global
    public required List<Type> Selection { get; init; } = [];
    public Type? SelectedType { get; private set; }
    public event Action<Type>? SelectedTypeChanged;
    
    public TypeDropdown()
    {
        InitializeComponent();
        
        Loaded += (_, _) =>
        {
            if (Items.Count > 0) return; 
            Items.Clear();
            
            foreach (var comboBoxItem in Selection.Select(type => new ComboBoxItem { Content = type.Name, Tag = type}))
            {
                comboBoxItem.Selected += ComboBoxItemSelected;
                Items.Add(comboBoxItem);
            }
            
            if (Items.Count == 0) return; 
            SelectedIndex = 0;
        };
    }

    public void Select(Type? type)
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
    
    private void ComboBoxItemSelected(object sender, EventArgs e)
    {
        var comboBoxItem = (ComboBoxItem)sender;
        SelectedType = (Type)comboBoxItem.Tag;
        SelectedTypeChanged?.Invoke(SelectedType);
    }
}