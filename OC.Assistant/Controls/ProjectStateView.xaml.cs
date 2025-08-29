using System.Windows;
using System.Windows.Media;
using OC.Assistant.Core;

namespace OC.Assistant.Controls;

public partial class ProjectStateView
{
    private bool _isSolution;
    
    public ProjectStateView()
    {
        InitializeComponent();
        IndicateDisconnected();
        
        ProjectState.Events.Connected += SetSolutionPath;
        ProjectState.Events.Disconnected += IndicateDisconnected;
        ProjectState.Events.StartedRunning += IndicateRunMode;
        ProjectState.Events.StoppedRunning += IndicateConfigMode;
    }

    private void SetSolutionPath(string projectFile, string? projectFolder)
    {
        ProjectLabel.Content = projectFile;
        _isSolution = projectFolder is not null;
    }

    private void IndicateDisconnected()
    {
        StateLabel.Content = null;
        HostLabel.Content = null;
        ProjectLabel.Content = null;
    }

    private void IndicateRunMode()
    {
        StateBorder.Background = Application.Current.Resources["Success1Brush"] as SolidColorBrush;
        StateLabel.Content = "Run";
        HostLabel.Content = _isSolution ? $"NetId:{Sdk.ApiLocal.Interface.NetId}" : "local file";
    }

    private void IndicateConfigMode()
    {
        StateBorder.Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
        StateLabel.Content = "Config";
        HostLabel.Content = _isSolution ? $"NetId:{Sdk.ApiLocal.Interface.NetId}" : "local file";
    }
}