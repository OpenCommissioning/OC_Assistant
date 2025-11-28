using System.Windows;

namespace OC.Assistant.Controls;

internal partial class FileMenu
{
    public FileMenu()
    {
        InitializeComponent();
        AppControl.Instance.Connected += _ => DisconnectItem.IsEnabled = true;
        AppControl.Instance.Disconnected += () => DisconnectItem.IsEnabled = false;
    }

    private void CloseProjectOnClick(object sender, RoutedEventArgs e) => AppControl.Instance.Disconnect();
    
    private void ExitOnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}