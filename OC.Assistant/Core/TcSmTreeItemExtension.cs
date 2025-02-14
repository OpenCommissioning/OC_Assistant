using System.Diagnostics.CodeAnalysis;
using TCatSysManagerLib;

namespace OC.Assistant.Core;

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
        return parent.Cast<ITcSmTreeItem>().FirstOrDefault(c => c.Name == childName);
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
        var child = parent.TryLookupChild(childName);
        if (child is null) return null;
        return child.ItemType == (int) type ? child : null;
    }
    
    /// <summary>
    /// Tries to find a <see cref="ITcSmTreeItem"/> recursive.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="childName">The name of the child item.</param>
    /// <param name="type">The <see cref="TREEITEMTYPES"/> of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? FindChildRecursive(this ITcSmTreeItem parent, string? childName, TREEITEMTYPES type)
    {
        foreach (ITcSmTreeItem child in parent)
        {
            if (child.Name.Equals(childName, StringComparison.CurrentCultureIgnoreCase) && child.ItemType == (int)type)
            {
                return child;
            }

            var grandChild = FindChildRecursive(child, childName, type);
            if (grandChild is not null)
            {
                return grandChild;
            }
        }
        
        return null;
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
        var item = parent.TryLookupChild(childName, type);
        if (item is not null) return item;
        Thread.Sleep(1); //"Breathing room" for the COM interface
        return parent.CreateChild(childName, nSubType: (int)type);
    }
    
    /// <summary>
    /// Creates or overwrites a GVL item with the given content.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="name">The name of the GVL. 'GVL_' is automatically added at the start.</param>
    /// <param name="variables">The variable declarations.</param>
    /// <returns>The GVL <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? CreateGvl(this ITcSmTreeItem? parent, string name, string? variables)
    {
        if (parent?.GetOrCreateChild($"GVL_{name}", TREEITEMTYPES.TREEITEMTYPE_PLCGVL) is not { } gvlItem)
        {
            return null;
        }

        if (gvlItem is not ITcPlcDeclaration gvlDecl)
        {
            return null;
        }
        
        gvlDecl.DeclarationText = 
            $"{{attribute 'qualified_only'}}\n{{attribute 'subsequent'}}\nVAR_GLOBAL\n{variables}END_VAR";
        
        return gvlItem;
    }

    /// <summary>
    /// Creates or overwrites a DUT struct item with the given content.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="name">The name of the DUT. 'ST_' is automatically added at the start.</param>
    /// <param name="variables">The variable declarations.</param>
    /// <returns>The DUT struct <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? CreateDutStruct(this ITcSmTreeItem? parent, string name, string? variables)
    {
        if (parent?.GetOrCreateChild($"ST_{name}", TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not { } dutItem)
        {
            return null;
        }

        if (dutItem is not ITcPlcDeclaration dutDecl)
        {
            return null;
        }
        
        dutDecl.DeclarationText = 
            $"{{attribute 'pack_mode' := '0'}}\nTYPE ST_{name} :\nSTRUCT\n{variables}END_STRUCT\nEND_TYPE"; 
        
        return dutItem;
    }
}