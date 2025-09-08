using System.Windows;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

public partial class TcpIpServer
{
    private readonly Core.TcpIpServer _tcpIpServer = new ();
    
    public TcpIpServer()
    {
        InitializeComponent();
    }

    public void Activate()
    {
        _tcpIpServer.RunDetached(XmlFile.Instance.TcpIpServerAddress, XmlFile.Instance.TcpIpServerPort);
        Visibility = Visibility.Visible;
    }
    
    public void Deactivate()
    {
        Visibility = Visibility.Collapsed;
        _ = _tcpIpServer.CloseAsync();
    }

    private async void SettingsOnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = new TcpIpServerSettings();
            if (await Theme.MessageBox.Show("Server settings", settings, MessageBoxButton.OKCancel, MessageBoxImage.None,
                    settings.Save) != MessageBoxResult.OK) return;
            
            await _tcpIpServer.CloseAsync();
            _tcpIpServer.RunDetached(XmlFile.Instance.TcpIpServerAddress, XmlFile.Instance.TcpIpServerPort);
        }
        catch (Exception ex)
        {
            Logger.LogError(this, ex.Message);
        }
    }
}