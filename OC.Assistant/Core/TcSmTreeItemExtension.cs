using TCatSysManagerLib;

namespace OC.Assistant.Core;

public static class TcSmTreeItemExtension
{
    /// <summary>
    /// Tries to cast the given <see cref="ITcSmTreeItem"/> to the specified type.
    /// </summary>
    /// <typeparam name="T">The target class or interface to cast to.</typeparam>
    /// <param name="item">The <see cref="ITcSmTreeItem"/> to cast.</param>
    /// <returns>An instance of type <typeparamref name="T"/> if the cast is successful, otherwise null.</returns>
    public static T? CastTo<T>(this ITcSmTreeItem? item) where T : class => item as T;
    
    /// <summary>
    /// Searches for a child in the given <see cref="ITcSmTreeItem"/> by name and type.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="childName">The name of the child item.</param>
    /// <param name="type">The type of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? GetChild(this ITcSmTreeItem? parent, string? childName, TREEITEMTYPES type)
    {
        if (parent is null) return null;
        var nameIsUnknown = string.IsNullOrEmpty(childName);
        var typeIsUnknown = type == TREEITEMTYPES.TREEITEMTYPE_UNKNOWN;
        if (nameIsUnknown && typeIsUnknown) return null;
        
        foreach (ITcSmTreeItem item in parent)
        {
            var nameIsEqual = string.Equals(item.Name, childName, StringComparison.OrdinalIgnoreCase);
            var typeIsEqual = item.ItemType == (int)type;
            
            if (nameIsEqual && typeIsEqual || nameIsEqual && typeIsUnknown || typeIsEqual && nameIsUnknown)
            {
                ComHelper.TrackObject(item);
                return item;
            }

            ComHelper.ReleaseObject(item);
        }

        return null;
    }
    
    /// <summary>
    /// Searches for a child in the given <see cref="ITcSmTreeItem"/> by name.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="childName">The name of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? GetChild(this ITcSmTreeItem? parent, string? childName) => 
        GetChild(parent, childName, TREEITEMTYPES.TREEITEMTYPE_UNKNOWN);
    
    /// <summary>
    /// Searches for a child in the given <see cref="ITcSmTreeItem"/> by type.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/></param>
    /// <param name="type">The type of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? GetChild(this ITcSmTreeItem? parent, TREEITEMTYPES type) => 
        GetChild(parent, null, type);
    
    /// <summary>
    /// Gets the child items as an enumeration of type <see cref="ITcSmTreeItem"/>.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <returns>An enumeration of type <see cref="ITcSmTreeItem"/>, if any.</returns>
    public static IEnumerable<ITcSmTreeItem> GetChildren(this ITcSmTreeItem? parent)
    {
        if (parent is null) yield break;
        foreach (ITcSmTreeItem item in parent)
        {
            ComHelper.TrackObject(item);
            yield return item;
        }
    }
    
    /// <summary>
    /// Tries to find a <see cref="ITcSmTreeItem"/> recursive.
    /// </summary>
    /// <param name="parent">The parent <see cref="ITcSmTreeItem"/>.</param>
    /// <param name="childName">The name of the child item.</param>
    /// <param name="type">The <see cref="TREEITEMTYPES"/> of the child item.</param>
    /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
    public static ITcSmTreeItem? GetChildRecursive(this ITcSmTreeItem? parent, string? childName, TREEITEMTYPES type)
    {
        if (parent is null) return null;
        
        if (parent.GetChild(childName, type) is {} item)
        {
            return item;       
        }
        
        foreach (ITcSmTreeItem child in parent)
        {
            if (child.GetChildRecursive(childName, type) is {} grandChild)
            {
                ComHelper.ReleaseObject(child);
                return grandChild;       
            }
            ComHelper.ReleaseObject(child);
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
    public static ITcSmTreeItem? GetOrCreateChild(this ITcSmTreeItem? parent, string? childName, TREEITEMTYPES type)
    {
        var compatibleName = childName?.MakePlcCompatible();
        var item = parent.GetChild(compatibleName, type);
        if (item is not null) return item;
        var child = parent?.CreateChild(compatibleName, nSubType: (int)type);
        ComHelper.TrackObject(child);
        return child;
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
        if (parent.GetOrCreateChild($"GVL_{name}", TREEITEMTYPES.TREEITEMTYPE_PLCGVL) is not { } gvlItem)
        {
            return null;
        }

        if (gvlItem.CastTo<ITcPlcDeclaration>() is not {} gvlDecl)
        {
            return null;
        }
        
        gvlDecl.DeclarationText = 
            $"{{attribute 'linkalways'}}\n" +
            $"{{attribute 'qualified_only'}}\n" +
            $"{{attribute 'subsequent'}}\n" +
            $"VAR_GLOBAL\n{variables}END_VAR";
        
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
        if (parent.GetOrCreateChild($"ST_{name}", TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not { } dutItem)
        {
            return null;
        }

        if (dutItem.CastTo<ITcPlcDeclaration>() is not {} dutDecl)
        {
            return null;
        }
        
        dutDecl.DeclarationText = 
            $"{{attribute 'pack_mode' := '0'}}\nTYPE ST_{name} :\nSTRUCT\n{variables}END_STRUCT\nEND_TYPE"; 
        
        return dutItem;
    }
}