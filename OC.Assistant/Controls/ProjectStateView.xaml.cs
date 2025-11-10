using System.Windows;
using System.Windows.Media;
using OC.Assistant.Sdk;

namespace OC.Assistant.Controls;

public partial class ProjectStateView
{
    public ProjectStateView()
    {
        InitializeComponent();
        IndicateDisconnected();
        
        AppControl.Instance.Connected += SetSolutionPath;
        AppControl.Instance.Disconnected += IndicateDisconnected;
        AppControl.Instance.StartedRunning += IndicateRunMode;
        AppControl.Instance.StoppedRunning += IndicateConfigMode;
    }

    private void SetSolutionPath(string projectFile, CommunicationType mode, object? parameter)
    {
        ProjectLabel.Content = projectFile;
        ModeLabel.Content = mode.ToString();
        IndicateConfigMode();
    }

    private void IndicateDisconnected()
    {
        ProjectLabel.Content = null;
        ModeLabel.Content = null;
        StateLabel.Content = null;
    }

    private void IndicateRunMode()
    {
        StateBorder.Background = Application.Current.Resources["Success1Brush"] as SolidColorBrush;
        StateLabel.Content = "Run";
    }

    private void IndicateConfigMode()
    {
        StateBorder.Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
        StateLabel.Content = "Config";
    }
}