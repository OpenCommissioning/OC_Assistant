using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Models;

public class Plugin() : IPlugin, IDisposable
{
    public enum State { Idle, Starting, Run, Stopping }
    
    public string Name { get; set; } = "MyPlugin";
    public Type? Type { get; private set; }
    public Type? ChannelType { get; set; }
    public IPluginController? PluginController { get; private set; }
    
    [MemberNotNullWhen(true, nameof(Type))]
    [MemberNotNullWhen(true, nameof(PluginController))]
    public bool IsValid => Type is not null && PluginController is not null;

    public Plugin(string name, Type? type, Type? channelType, XContainer parameter) : this()
    {
        if (!InitType(type)) return;
        Type = type;
        ChannelType = channelType;
        PluginController?.Parameter.Update(parameter);
        Name = name;
        PluginController?.Initialize(name);
    }

    public async Task<bool> SaveAsync(string name)
    {
        if (!IsValid) return false;
        Name = name;
        PluginController.Stop();
        if (!await Task.Run(() => PluginController.Save(name))) return false;
        Saved?.Invoke();
        return true;
    }
        
    [MemberNotNullWhen(true, nameof(Type))]
    [MemberNotNullWhen(true, nameof(PluginController))]
    public bool InitType(Type? type)
    {
        try
        {
            if (type is null)
            {
                throw new Exception($"{type} is null");
            }

            if (PluginController is not null)
            {
                PluginController.Stop();
                PluginController.ChannelRequested -= PluginOnChannelRequested;
                PluginController.Starting -= PluginOnStarting;
                PluginController.Started -= PluginOnStarted;
                PluginController.Stopping -= PluginOnStopping;
                PluginController.Stopped -= PluginOnStopped;
            }
            
            Type = type;
            PluginController = Activator.CreateInstance(type) as IPluginController;
            if (PluginController is null) return false;
            
            PluginController.ChannelRequested += PluginOnChannelRequested;
            PluginController.Starting += PluginOnStarting;
            PluginController.Started += PluginOnStarted;
            PluginController.Stopping += PluginOnStopping;
            PluginController.Stopped += PluginOnStopped;
            
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return false;
        }
    }
    
    private ChannelBase? PluginOnChannelRequested()
    {
        if (PluginController is null || ChannelType is null) return null;
        
        var readSize = PluginController.InputStructure.Length;
        var writeSize = PluginController.OutputStructure.Length;

        try
        {
            return Activator.CreateInstance(ChannelType, writeSize, readSize) as ChannelBase;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return null;
        }
    }
    
    public event Action<State>? StateChanged;
    public event Action? Saved;

    private void PluginOnStarting() => StateChanged?.Invoke(State.Starting);
    private void PluginOnStarted() => StateChanged?.Invoke(State.Run);
    private void PluginOnStopping() => StateChanged?.Invoke(State.Stopping);
    private void PluginOnStopped() => StateChanged?.Invoke(State.Idle);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        PluginController?.Stop();
    }
}