using EnvDTE;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Core;

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
        var project = sysManager.VsProject as Project;
        project?.Save();
        project?.Finalize();
    }
    
    /// <summary>
    /// Gets an <see cref="ITcSmTreeItem"/> by the given name.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    /// <param name="rootItemName">The name of the root <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="name">The name or path of the <see cref="ITcSmTreeItem"/> to find.</param>
    public static ITcSmTreeItem? TryGetItem(this ITcSysManager15 sysManager, string rootItemName, string? name = null)
    {
        var path = rootItemName;
        if (!string.IsNullOrEmpty(name))
        {
            path += $"^{name}";
        }
        if (sysManager.TryLookupTreeItem(path, out var item))
        {
            ComObjects.Add(item);
            return item;
        }
        Logger.LogError(typeof(TcSysManagerExtension), $"{path} not found");
        return null;
    }
    
    /// <summary>
    /// Gets an enumeration of type <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    /// <param name="rootItemName">The name of the root <see cref="ITcSmTreeItem"/>.</param>
    public static IEnumerable<ITcSmTreeItem> TryGetItems(this ITcSysManager15 sysManager, string rootItemName)
    {
        if (sysManager.TryGetItem(rootItemName) is not {} rootItem)
        {
            yield break;
        }
        
        foreach (ITcSmTreeItem item in rootItem)
        {
            ComObjects.Add(item);
            yield return item;
        }
    }

    /// <summary>
    /// Gets the plc project as <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static ITcSmTreeItem? TryGetPlcProject(this ITcSysManager15 sysManager)
    {
        var plc = sysManager.TryGetItem(TcShortcut.PLC, XmlFile.Instance.PlcProjectName);
        var item = plc?.TryLookupChild(null, TREEITEMTYPES.TREEITEMTYPE_PLCAPP);
        return item;
    }
        
    /// <summary>
    /// Gets the plc instance as <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static ITcSmTreeItem? TryGetPlcInstance(this ITcSysManager15 sysManager)
    {
        var plc = sysManager.TryGetItem(TcShortcut.PLC, XmlFile.Instance.PlcProjectName);
        var item = plc?.TryLookupChild(null, TREEITEMTYPES.TREEITEMTYPE_TCOMPLCOBJECT);
        return item;
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
        if (!sysManager.TryLookupTreeItem(TcShortcut.IO_DEVICE, out var ioDevice)) return null;
        ComObjects.Add(ioDevice);
        
        if (sysManager.TryGetItem(TcShortcut.IO_DEVICE, deviceName) is not null)
        {
            ioDevice.DeleteChild(deviceName);
        }
        
        var device = ioDevice.ImportChild(xtiFilePath);
        ComObjects.Add(device);
        return device;
    }
}