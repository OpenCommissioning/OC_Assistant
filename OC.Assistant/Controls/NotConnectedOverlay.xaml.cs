using System.Windows;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;

namespace OC.Assistant.Controls;

public partial class NotConnectedOverlay : IConnectionState
{
    public NotConnectedOverlay()
    {
        InitializeComponent();
        ProjectManager.Instance.Subscribe(this);
    }

    void IConnectionState.OnConnect()
    {
        Visibility = Visibility.Hidden;
    }

    void IConnectionState.OnDisconnect()
    {
        Visibility = Visibility.Visible;
    }
    
    private void DteOnSelected(TcDte dte)
    {
        FileMenu.ConnectSolution(dte);
    }

    private void OpenOnClick(object sender, RoutedEventArgs e)
    {
        FileMenu.OpenSolution();
    }
    
    private void CreateOnClick(object sender, RoutedEventArgs e)
    {
        FileMenu.CreateSolution();
    }
}