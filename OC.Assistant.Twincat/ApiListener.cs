using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Twincat.Automation;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat;

public class ApiListener
{
    private static readonly Lazy<ApiListener> Lazy = new(() => new ApiListener());
    
    public static ApiListener Singleton => Lazy.Value;
    
    private ApiListener()
    {
        if (Lazy.IsValueCreated) return;
        EventSystem.ApiDataReceived += OnApiDataReceived;
    }
    
    public event Action? AppDisconnected;
    
    private void OnApiDataReceived(string identifier, XElement payload)
    {
        switch (identifier)
        {
            case "app/disconnected":
                AppDisconnected?.Invoke();
                return;
            case "app/pluginChanged":
                var channel = payload.Element("Channel")?.Value;
                var add = payload.Element("Add")?.Value;
                var delete = payload.Element("Delete")?.Value;
                PluginUpdate(channel, add, delete);
                return;
            case "data/config":
                UpdateConfig(payload.FirstNode as XElement);
                return;
        }
    }
    
    private void PluginUpdate(string? channel, string? add, string? delete)
    {
        if (channel != nameof(TcAdsChannel)) return;
        
        var plugin = XmlFile
            .Instance
            .GetPluginElements()
            .FirstOrDefault(x => x.Name == add);
        
        if (plugin is null && delete is null) return;

        if (TcState.Singleton.AdsState == AdsState.Run)
        {
            Logger.LogWarning(this, "TwinCAT is in Run Mode, no project modification.");
            return;
        }
        
        DteSingleThread.Run(tcSysManager =>
        {
            tcSysManager.SaveProject();
            if (tcSysManager.GetPlcProject() is not { } plcProjectItem)
            {
                Logger.LogError(this, "No Plc project found");
                return;
            }
            SilGenerator.Update(plcProjectItem, plugin);
            SilGenerator.Remove(plcProjectItem, delete);
        });
    }
    
    private void UpdateConfig(XElement? config)
    {
        if (config is null) return;
        
        XmlFile.Instance.Main = config;
        XmlFile.Instance.Save();
        
        if (TcState.Singleton.AdsState == AdsState.Run)
        {
            throw new Exception("TwinCAT is in Run Mode, no project modification.");
        }
        
        DteSingleThread.Run(tcSysManager =>
        {
            tcSysManager.SaveProject();
            if (tcSysManager.GetPlcProject() is not { } plcProjectItem)
            {
                throw new Exception("No Plc project found");
            }
            ProjectGenerator.Update(plcProjectItem);
            Logger.LogInfo(this, "Project update finished.");
        }, throwExceptions: true);
    }
}