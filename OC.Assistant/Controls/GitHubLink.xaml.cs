using System.Diagnostics;
using System.Windows;

namespace OC.Assistant.Controls;

public partial class GitHubLink
{
    public GitHubLink()
    {
        InitializeComponent();
    }

    private void GitHubLinkOnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/OpenCommissioning/OC_Assistant", 
            UseShellExecute = true
        });
    }
}