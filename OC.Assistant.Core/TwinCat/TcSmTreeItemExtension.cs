using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using TCatSysManagerLib;

namespace OC.Assistant.Core.TwinCat;

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
public static class TcSmTreeItemExtension
{
    /// <summary>
    /// Searches for a child in the given tree by name.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/></param>
    /// <param name="childName">The name of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? TryLookupChild(this ITcSmTreeItem parent, string? childName)
    {
        return Retry.Invoke(() =>
        {
            return parent.Cast<ITcSmTreeItem>().FirstOrDefault(c => c.Name == childName);
        });
    }

    /// <summary>
    /// Searches for a child in the given tree by name and type.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/></param>
    /// <param name="childName">The name of the child item.</param>
    /// <param name="type">The <see cref="TREEITEMTYPES"/> of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? TryLookupChild(this ITcSmTreeItem parent, string? childName, TREEITEMTYPES type)
    {
        return Retry.Invoke(() =>
        {
            var child = parent.TryLookupChild(childName);
            if (child is null) return null;
            return child.ItemType == (int) type ? child : null;
        });
    }
    
    /// <summary>
    /// Gets a child in the given tree by name and type. Creates the child if not existent.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/></param>
    /// <param name="childName">The name of the child item.</param>
    /// <param name="type">The <see cref="TREEITEMTYPES"/> of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? GetOrCreateChild(this ITcSmTreeItem parent, string? childName, TREEITEMTYPES type)
    {
        return Retry.Invoke(() =>
        {
            var item = parent.TryLookupChild(childName, type) ?? parent.CreateChild(childName, nSubType: (int)type);
            return item;
        });
    }
    
    /// <summary>
    /// Finds the MAIN program.
    /// </summary>
    /// <param name="plcProjectItem">The plc project as <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="main">The main program as <see cref="ITcSmTreeItem"/></param>
    /// <returns>True if successful, otherwise false.</returns>
    public static bool FindMain(this ITcSmTreeItem plcProjectItem, out ITcSmTreeItem? main)
    {
        ITcSmTreeItem? foundItem = null;

        var success = Retry.Invoke(() =>
        {
            foreach (ITcSmTreeItem item in plcProjectItem)
            {
                if (item.Name.Equals("MAIN", StringComparison.CurrentCultureIgnoreCase) && item.ItemType == (int)TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG)
                {
                    foundItem = item;
                    return true;
                }

                if (FindMain(item, out foundItem)) return true;
            }

            return false;
        });

        main = foundItem;
        return success;
    }
    
    /// <summary>
    /// Creates a zip file from given folder and integrates into the TwinCAT project.
    /// </summary>
    /// <param name="plcProjectItem">The <see cref="ITcSmTreeItem"/> of the plc project.</param>
    /// <param name="folderName">The name of the folder to be zipped and integrated.</param>
    public static void TcIntegrate(this ITcSmTreeItem plcProjectItem, string folderName)
    {
        var folder = $"{AppData.Path}\\{folderName}";
        Directory.CreateDirectory(folder);
        
        //If folder is empty, do nothing
        if (Directory.GetDirectories(folder).Length == 0)
        {
            Directory.Delete(folder, true);
            return;
        }

        //Create ZIP file and delete temporary folder
        var zipFile = folder + ".zip";
        if (File.Exists(zipFile)) File.Delete(zipFile);
        ZipFile.CreateFromDirectory(folder, zipFile);
        Directory.Delete(folder, true);

        Retry.Invoke(() =>
        {
            //Import ZIP file to TwinCAT and delete
            var plcFolder = plcProjectItem.GetOrCreateChild(folderName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
            plcFolder?.ImportChild(zipFile);
            File.Delete(zipFile);

            //Remove the plc folder if it is empty
            if (plcFolder?.ChildCount == 0) plcProjectItem.DeleteChild(folderName);
        });
    }
}