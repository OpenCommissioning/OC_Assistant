using System.Diagnostics;
using System.Windows;

namespace OC.Assistant.Controls;

public partial class DownloadLink
{
    public DownloadLink(string latestVersion)
    {
        InitializeComponent();
        LatestVersion.Content = $"See latest Release {latestVersion} on";
    }
    
    private void HyperlinkOnClick(object sender, RoutedEventArgs routedEventArgs)
    {
        Process.Start(new ProcessStartInfo("https://github.com/opencommissioning/oc_assistant/releases/latest") { UseShellExecute = true });
    }
}