using Avalonia.Controls;

namespace OC.Assistant.Twincat.Views;

public partial class PnScannerMenu : MenuItem
{
    public PnScannerMenu()
    {
        InitializeComponent();
    }
    
    protected override Type StyleKeyOverride => typeof(MenuItem);
}
