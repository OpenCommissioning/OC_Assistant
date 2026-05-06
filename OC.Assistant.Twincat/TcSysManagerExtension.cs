using EnvDTE;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat;

/// <summary>
/// Extension for the <see cref="ITcSysManager15"/> interface.
/// </summary>
public static class TcSysManagerExtension
{
    extension(ITcSysManager15 sysManager)
    {
        /// <summary>
        /// Saves the <see cref="Project"/> associated with the given <see cref="ITcSysManager15"/>.
        /// </summary>
        public void SaveProject()
        {
            var project = sysManager.VsProject as Project;
            project?.Save();
            TcDte.ReleaseObject(project);
        }

        /// <summary>
        /// Gets an <see cref="ITcSmTreeItem"/> by the given path.
        /// </summary>
        /// <param name="itemPath">The path of the <see cref="ITcSmTreeItem"/>.</param>
        public ITcSmTreeItem? GetItem(string itemPath)
        {
            if (sysManager.TryLookupTreeItem(itemPath, out var item))
            {
                TcDte.TrackObject(item);
                return item;
            }
            Logger.LogError(typeof(TcSysManagerExtension), $"{itemPath} not found");
            return null;
        }

        /// <summary>
        /// Gets the plc project <see cref="ITcSmTreeItem"/>.
        /// </summary>
        public ITcSmTreeItem? GetPlcProject()
        {
            return sysManager
                .GetItem($"{TcShortcut.NODE_PLC_CONFIG}^{XmlFile.Instance.PlcProjectName}")?
                .GetChild(TREEITEMTYPES.TREEITEMTYPE_PLCAPP);
        }

        /// <summary>
        /// Gets the plc instance <see cref="ITcSmTreeItem"/>.
        /// </summary>
        public ITcSmTreeItem? GetPlcInstance()
        {
            return sysManager
                .GetItem($"{TcShortcut.NODE_PLC_CONFIG}^{XmlFile.Instance.PlcProjectName}")?
                .GetChild(TREEITEMTYPES.TREEITEMTYPE_TCOMPLCOBJECT);
        }

        /// <summary>
        /// Updates or creates an IoDevice.
        /// </summary>
        /// <param name="deviceName">The root name of the IoDevice.</param>
        /// <param name="xtiFilePath">The xti file path of the IoDevice.</param>
        /// <returns>The updated or created IoDevice as <see cref="ITcSmTreeItem"/>.</returns>
        public ITcSmTreeItem? UpdateIoDevice(string deviceName, string xtiFilePath)
        {
            if (sysManager.GetItem(TcShortcut.NODE_IO_DEVICES) is not {} ioDevice) return null;
        
            if (ioDevice.GetChild(deviceName) is not null)
            {
                ioDevice.DeleteChild(deviceName);
            }
        
            var device = ioDevice.ImportChild(xtiFilePath);
            TcDte.TrackObject(device);
            return device;
        }
    }
}