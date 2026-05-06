using System.Collections.ObjectModel;
using System.Xml.Linq;
using OC.Assistant.Models;
using OC.Assistant.Sdk;

namespace OC.Assistant.Services;

public class PluginService
{
    public ObservableCollection<Plugin> Plugins { get; } = [];
    
    public PluginService()
    {
        XmlFile.Instance.Reloaded += XmlOnReloaded;
    }
    
    private void XmlOnReloaded()
    {
        StopAndRemove();
        foreach (var plugin in XmlFile.Instance.LoadPlugins())
        {
            Plugins.Add(plugin);
        }
    }

    public void StopAndRemove()
    {
        foreach (var plugin in Plugins)
        {
            plugin.PluginController?.Stop();
            plugin.Dispose();
        }
        
        Plugins.Clear();
    }
    
    public void StartPlugins(Type? channelType= null)
    {
        if (BusyState.IsSet) return;

        Task.Run(async () =>
        {
            foreach (var plugin in Plugins)
            {
                if (plugin.PluginController?.AutoStart != true) continue;
                if (channelType is not null && plugin.ChannelType != channelType) continue;
                plugin.PluginController.Start();
                await Task.Delay(plugin.PluginController?.DelayAfterStart ?? 0);
            }
        });
    }
    
    public void StopPlugins(Type? channelType = null)
    {
        if (BusyState.IsSet) return;

        Task.Run(async () =>
        {
            foreach (var plugin in Plugins)
            {
                if (channelType is not null && plugin.ChannelType != channelType) continue;
                plugin.PluginController?.Stop();
                await Task.Delay(100);
            }
        });
    }
    
    public void SavePlugin(Plugin plugin, string? oldName, Type? oldChannel)
    {
        if (Plugins.FirstOrDefault(x => x.Equals(plugin)) is null)
        {
            Plugins.Add(plugin);
            oldName = null;
        }
        
        XmlFile.Instance.UpdatePlugin(plugin, oldName);
        
        if (plugin.PluginController is null) return;
        if (!plugin.PluginController.IoChanged && oldName is null && plugin.ChannelType == oldChannel) return;
        plugin.PluginController?.Stop();
        PluginChangedEvent(plugin.ChannelType, plugin.Name, oldName);
    }

    public void ResetPlugin(Plugin plugin)
    {
        XmlFile.Instance.LoadPluginParameters(plugin);
    }
    
    public void RemovePlugin(Plugin plugin)
    {
        XmlFile.Instance.RemovePlugin(plugin.Name);
        Plugins.Remove(plugin);
        plugin.Dispose();
        PluginChangedEvent(plugin.ChannelType, null, plugin.Name);
    }

    private static void PluginChangedEvent(Type? channel, string? add, string? delete)
    {
        var payload = new XElement("Payload");
        if (channel is not null) payload.Add(new XElement("Channel", channel.Name));
        if (add is not null) payload.Add(new XElement("Add", add));
        if (delete is not null) payload.Add(new XElement("Delete", delete));
        EventSystem.InvokeApiEvent("app/pluginChanged", payload);
    }
}