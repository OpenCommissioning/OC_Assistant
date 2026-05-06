using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OC.Assistant.PluginServer.Views;
using OC.Assistant.Sdk;
using OC.Assistant.Ui;

namespace OC.Assistant.PluginServer.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    public MenuViewModel()
    {
        EventSystem.ApiDataReceived += EventSystemOnApiDataReceived;
    }
    
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    [ObservableProperty] 
    public partial string IpAddress { get; set; } = "127.0.0.1";

    [ObservableProperty] 
    public partial string Port { get; set; } = "50100";

    private async void EventSystemOnApiDataReceived(string identifier, XElement payload)
    {
        try
        {
            switch (identifier)
            {
                case "app/connected":
                    IsEnabled = true;
                    TcpIpServer.RunDetached();
                    return;
                case "app/disconnected":
                case "app/closed":
                    IsEnabled = false;
                    await TcpIpServer.CloseAsync();
                    return;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }

    [RelayCommand]
    private async Task Settings()
    {
        if (XmlFile.Instance.Path is null) return;
        IpAddress = XmlFile.Instance.IpAddress;
        Port = XmlFile.Instance.Port.ToString();
        
        var settings = new Settings{DataContext = this};

        if (!await MessageBox.Show(
                settings, 
                MessageBoxButton.OkCancel, 
                MessageBoxImage.None, 
                SaveSettings)) return;
        
        await TcpIpServer.CloseAsync();
        TcpIpServer.RunDetached();
    }

    private bool SaveSettings()
    {
        if (XmlFile.Instance.Path is null) return false;
        XmlFile.Instance.IpAddress = IpAddress;
        XmlFile.Instance.Port = int.TryParse(Port, out var port) ? port : 50100;
        XmlFile.Instance.Save();
        return true;
    }
}