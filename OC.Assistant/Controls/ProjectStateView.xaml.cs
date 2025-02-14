using System.Windows;
using System.Windows.Media;

namespace OC.Assistant.Controls;

public partial class ProjectStateView
{
    public ProjectStateView()
    {
        InitializeComponent();
        IndicateDisconnected();
    }
    
    protected void SetSolutionPath(string? path)
    {
        Dispatcher.Invoke(() =>
        {
            SolutionLabel.Content = path;
        });
    }

    protected void IndicateDisconnected()
    {
        Dispatcher.Invoke(() =>
        {
            StateBorder.Background = Application.Current.Resources["White4Brush"] as SolidColorBrush;
            StateLabel.Content = "Offline";
            NedIdLabel.Content = null;
            SolutionLabel.Content = null;
        });
    }

    protected void IndicateRunMode()
    {
        Dispatcher.Invoke(() =>
        {
            StateBorder.Background = Application.Current.Resources["Green1Brush"] as SolidColorBrush;
            StateLabel.Content = "Run";
            NedIdLabel.Content = Sdk.ApiLocal.Interface.NetId;
        });
    }

    protected void IndicateConfigMode()
    {
        Dispatcher.Invoke(() =>
        {
            StateBorder.Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
            StateLabel.Content = "Config";
            NedIdLabel.Content = Sdk.ApiLocal.Interface.NetId;
        });
    }
}