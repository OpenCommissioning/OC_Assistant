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
        ComHelper.ReleaseObject(project);
    }
    
    /// <summary>
    /// Gets an <see cref="ITcSmTreeItem"/> by the given name.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    /// <param name="rootItemName">The name of the root <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="name">The name or path of the <see cref="ITcSmTreeItem"/> to find.</param>
    public static ITcSmTreeItem? GetItem(this ITcSysManager15 sysManager, string rootItemName, string? name = null)
    {
        var path = rootItemName;
        if (!string.IsNullOrEmpty(name))
        {
            path += $"^{name}";
        }
        if (sysManager.TryLookupTreeItem(path, out var item))
        {
            ComHelper.TrackObject(item);
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
    public static IEnumerable<ITcSmTreeItem> GetItems(this ITcSysManager15 sysManager, string rootItemName)
    {
        if (sysManager.GetItem(rootItemName) is not {} rootItem)
        {
            yield break;
        }
        
        foreach (ITcSmTreeItem item in rootItem)
        {
            ComHelper.TrackObject(item);
            yield return item;
        }
    }

    /// <summary>
    /// Gets the plc project as <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static ITcSmTreeItem? GetPlcProject(this ITcSysManager15 sysManager)
    {
        var plc = sysManager.GetItem(TcShortcut.PLC, XmlFile.Instance.PlcProjectName);
        var item = plc?.GetChild(TREEITEMTYPES.TREEITEMTYPE_PLCAPP);
        return item;
    }
        
    /// <summary>
    /// Gets the plc instance as <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="sysManager">The <see cref="ITcSysManager15"/> interface.</param>
    public static ITcSmTreeItem? GetPlcInstance(this ITcSysManager15 sysManager)
    {
        var plc = sysManager.GetItem(TcShortcut.PLC, XmlFile.Instance.PlcProjectName);
        var item = plc?.GetChild(TREEITEMTYPES.TREEITEMTYPE_TCOMPLCOBJECT);
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
        if (sysManager.GetItem(TcShortcut.IO_DEVICE) is not {} ioDevice) return null;
        
        if (ioDevice.GetChild(deviceName) is not null)
        {
            ioDevice.DeleteChild(deviceName);
        }
        
        var device = ioDevice.ImportChild(xtiFilePath);
        ComHelper.TrackObject(device);
        return device;
    }
}