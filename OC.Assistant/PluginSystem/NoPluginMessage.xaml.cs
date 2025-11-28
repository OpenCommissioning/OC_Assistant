using System.Diagnostics;
using System.Windows;

namespace OC.Assistant.PluginSystem;

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