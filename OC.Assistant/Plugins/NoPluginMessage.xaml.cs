using System.Diagnostics;
using System.Windows;

namespace OC.Assistant.Plugins;

public partial class NoPluginMessage
{
    public NoPluginMessage()
    {
        InitializeComponent();
    }

    private void LinkButtonOnClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/OpenCommissioning/OC_Assistant",
            UseShellExecute = true
        });
    }
}