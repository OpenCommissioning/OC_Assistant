using Avalonia.Controls;

namespace OC.Assistant.Views;

public partial class MainMenu : Menu
{
    public MainMenu()
    {
        InitializeComponent();
        
        foreach (var item in AssemblyRegister.Menus)
        {
            if (item is not MenuItem menuItem) continue;
            menuItem.Bind(CornerRadiusProperty, this.GetResourceObservable("ControlCornerRadius"));
            Items.Insert(1, menuItem);
        }
    }
    
    protected override Type StyleKeyOverride => typeof(Menu);
}