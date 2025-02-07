using EnvDTE;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Core.TwinCat;

/// <summary>
/// Extension for the <see cref="ITcSysManager15"/> interface.
/// </summary>
public static class TcSysManagerExtension
{
    /// <summary>
    /// Saves the <see cref="Project"/> associated with the given <see cref="ITcSysManager15"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static void SaveProject(this ITcSysManager15 sysManager)
    {
        (sysManager.VsProject as Project)?.Save();
    }
    
    /// <summary>
    /// Gets an <see cref="ITcSmTreeItem"/> by the given name.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    /// <param name="rootItemName">The name of the root <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="name">The name of the <see cref="ITcSmTreeItem"/> to find.</param>
    public static ITcSmTreeItem? TryGetItem(this ITcSysManager15 sysManager, string rootItemName, string? name)
    {
        if (!sysManager.TryLookupTreeItem(rootItemName, out var rootItem))
        {
            Logger.LogError(typeof(TcSysManagerExtension), $"RootItem {rootItemName} not found");
            return null;
        }
        
        foreach (var item in rootItem.Cast<ITcSmTreeItem>()
                     .Where(item => name is null || item.Name == name))
        {
            return item;
        }
        
        Logger.LogError(typeof(TcSysManagerExtension), $"Item {name} not found in category {rootItemName}");
        return null;
    }

    /// <summary>
    /// Gets the plc project as <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static ITcSmTreeItem? TryGetPlcProject(this ITcSysManager15 sysManager)
    {
        return sysManager
            .TryGetItem(TcShortcut.PLC, XmlFile.Instance.PlcProjectName)?
            .Cast<ITcSmTreeItem>()
            .FirstOrDefault(item => item.ItemType == (int) TREEITEMTYPES.TREEITEMTYPE_PLCAPP);
    }
        
    /// <summary>
    /// Gets the plc instance as <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static ITcSmTreeItem? TryGetPlcInstance(this ITcSysManager15 sysManager)
    {
        return sysManager
            .TryGetItem(TcShortcut.PLC, XmlFile.Instance.PlcProjectName)?
            .Cast<ITcSmTreeItem>()
            .FirstOrDefault(item => item.ItemType == (int) TREEITEMTYPES.TREEITEMTYPE_TCOMPLCOBJECT);
    }

    /// <summary>
    /// Updates or creates an IoDevice.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    /// <param name="deviceName">The root name of the IoDevice.</param>
    /// <param name="xtiFilePath">The xti file path of the IoDevice.</param>
    /// <returns>The updated or created IoDevice as <see cref="ITcSmTreeItem"/>.</returns>
    public static ITcSmTreeItem? UpdateIoDevice(this ITcSysManager15 sysManager, string deviceName, string xtiFilePath)
    {
        if (sysManager.TryLookupTreeItem($"{TcShortcut.IO_DEVICE}^{deviceName}", out _))
        {
            sysManager.LookupTreeItem(TcShortcut.IO_DEVICE).DeleteChild(deviceName);
        }
        
        return sysManager.LookupTreeItem(TcShortcut.IO_DEVICE).ImportChild(xtiFilePath);
    }
}