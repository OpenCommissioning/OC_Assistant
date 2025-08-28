using System.Windows;

namespace OC.Assistant.Twincat;

public partial class NotConnectedOverlay
{
    public NotConnectedOverlay()
        => InitializeComponent();
    
    private void OpenOnClick(object sender, RoutedEventArgs e)
        => FileMenu.OpenSolution(sender, e);
    
    private void CreateOnClick(object sender, RoutedEventArgs e)
        => FileMenu.CreateSolution(sender, e);
}