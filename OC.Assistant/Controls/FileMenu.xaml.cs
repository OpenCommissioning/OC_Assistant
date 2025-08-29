using System.Windows;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

internal partial class FileMenu
{
    public FileMenu()
    {
        InitializeComponent();
        ProjectState.Events.Connected += (_, _) => DisconnectItem.IsEnabled = true;
        ProjectState.Events.Disconnected += () => DisconnectItem.IsEnabled = false;
    }

    private void CloseProjectOnClick(object sender, RoutedEventArgs e) => ProjectState.Control.Disconnect();
    
    private void ExitOnClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
}