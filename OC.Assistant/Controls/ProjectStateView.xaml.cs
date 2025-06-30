using System.Windows;
using System.Windows.Media;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

public partial class ProjectStateView
{
    public ProjectStateView()
    {
        InitializeComponent();
        IndicateDisconnected();
        
        ProjectState.Events.Connected += SetSolutionPath;
        ProjectState.Events.Disconnected += IndicateDisconnected;
        ProjectState.Events.StartedRunning += IndicateRunMode;
        ProjectState.Events.StoppedRunning += IndicateConfigMode;
    }

    private void SetSolutionPath(string? path)
    {
        SolutionLabel.Content = path;
    }

    private void IndicateDisconnected()
    {
        StateBorder.Background = Application.Current.Resources["White4Brush"] as SolidColorBrush;
        StateLabel.Content = "Offline";
        NedIdLabel.Content = null;
        SolutionLabel.Content = null;
    }

    private void IndicateRunMode()
    {
        StateBorder.Background = Application.Current.Resources["Success1Brush"] as SolidColorBrush;
        StateLabel.Content = "Run";
        NedIdLabel.Content = Sdk.ApiLocal.Interface.NetId;
    }

    private void IndicateConfigMode()
    {
        StateBorder.Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
        StateLabel.Content = "Config";
        NedIdLabel.Content = Sdk.ApiLocal.Interface.NetId;
    }
}