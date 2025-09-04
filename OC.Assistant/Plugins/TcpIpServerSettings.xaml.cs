using System.Net;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

public partial class TcpIpServerSettings
{
    public TcpIpServerSettings()
    {
        InitializeComponent();
        if (Core.XmlFile.Instance.Path is null) return;
        Address.Text = Core.XmlFile.Instance.TcpIpServerAddress;
        Port.Text = Core.XmlFile.Instance.TcpIpServerPort.ToString();
    }

    public bool Save()
    {
        if (Core.XmlFile.Instance.Path is null) return true;
        return Dispatcher.Invoke(() =>
        {
            if (!IPAddress.TryParse(Address.Text, out _))
            {
                Logger.LogError(this, $"{Address.Text} is not a valid IP address");
                return false;
            }

            if (!int.TryParse(Port.Text, out var number) || number < 0 || number > 65535)
            {
                Logger.LogError(this, $"{Port.Text} is not a valid port number");
                return false;
            }
            
            Core.XmlFile.Instance.TcpIpServerAddress = Address.Text;
            Core.XmlFile.Instance.TcpIpServerPort = number;
            return true;
        });
    }
}