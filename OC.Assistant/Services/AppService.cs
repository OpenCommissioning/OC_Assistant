using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Services;

public class AppService
{
    private readonly PluginService _pluginService;
    
    private string? _projectFile;
    public bool IsConnected => _projectFile is not null;
    
    public AppService(PluginService pluginService)
    {
        _pluginService = pluginService;
        EventSystem.AppDataReceived += OnAppDataReceived;
    }
    
    private void OnAppDataReceived(string identifier, object? payload)
    {
        App.InvokeOnMainThread(() =>
        {
            switch (identifier)
            {
                case "app/connect":
                    if (payload is string projectFile) Connect(projectFile);
                    return;
                case "app/disconnect":
                    Disconnect();
                    return;
                case "app/start":
                    _pluginService.StartPlugins(payload as Type);
                    return;
                case "app/stop":
                    _pluginService.StopPlugins(payload as Type);
                    return;
            }
        });
    }
    
    private void Connect(string projectFile)
    {
        if (_projectFile is not null) Disconnect();
        _projectFile = projectFile;
        XmlFile.Instance.Path = projectFile;
        _pluginService.StopPlugins();
        EventSystem.InvokeApiEvent("app/connected", new XElement("ProjectFile", _projectFile));
        Logger.LogInfo(this, $"{_projectFile} connected");
    }

    private void Disconnect()
    {
        if (_projectFile is null) return;
        Logger.LogInfo(this, $"{_projectFile} disconnected");
        _projectFile = null;
        _pluginService.StopAndRemove();
        EventSystem.InvokeApiEvent("app/disconnected");
    }
}