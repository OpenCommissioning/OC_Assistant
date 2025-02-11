using System.Windows;
using EnvDTE;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

public partial class NotConnectedOverlay
{
    public NotConnectedOverlay()
    {
        InitializeComponent();
        ProjectState.Events.Connected += OnConnect;
        ProjectState.Events.Disconnected += OnDisconnect;
    }

    private void OnConnect(string solutionFullName)
    {
        Visibility = Visibility.Hidden;
    }

    private void OnDisconnect()
    {
        Visibility = Visibility.Visible;
    }
    
    private void DteOnSelected(DTE dte)
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