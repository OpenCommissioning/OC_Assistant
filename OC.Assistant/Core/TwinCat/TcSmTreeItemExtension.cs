using System.Diagnostics.CodeAnalysis;
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
}