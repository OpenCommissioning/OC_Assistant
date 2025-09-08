using System.Windows;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

internal partial class FileMenu
{
    public FileMenu()
    {
        InitializeComponent();
        AppInterface.Instance.Connected += (_, _) => DisconnectItem.IsEnabled = true;
        AppInterface.Instance.Disconnected += () => DisconnectItem.IsEnabled = false;
    }

    private void CloseProjectOnClick(object sender, RoutedEventArgs e) => AppInterface.Instance.Disconnect();
    
    private void ExitOnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}