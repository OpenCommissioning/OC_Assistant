using TCatSysManagerLib;

namespace OC.Assistant.Twincat;

public static class TcSmTreeItemExtension
{
    extension(ITcSmTreeItem? parent)
    {
        /// <summary>
        /// Internal try catch for <see cref="ITcSmTreeItem.LookupChild"/>.
        /// </summary>
        private ITcSmTreeItem? TryLookupChild(string? childName)
        {
            try { return parent?.LookupChild(childName); }
            catch { return null; }
        }

        /// <summary>
        /// Tries to cast the given <see cref="ITcSmTreeItem"/> to the specified type.
        /// </summary>
        /// <typeparam name="T">The target class or interface to cast to.</typeparam>
        /// <returns>An instance of type <typeparamref name="T"/> if the cast is successful, otherwise null.</returns>
        public T? CastTo<T>() where T : class => parent as T;

        /// <summary>
        /// Searches for a child in the given <see cref="ITcSmTreeItem"/> by name and type.
        /// </summary>
        /// <param name="childName">The name of the child item.</param>
        /// <param name="type">The type of the child item.</param>
        /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? GetChild(string? childName, TREEITEMTYPES type)
        {
            if (parent is null) return null;
            var nameIsUnknown = string.IsNullOrEmpty(childName);
            var typeIsUnknown = type == TREEITEMTYPES.TREEITEMTYPE_UNKNOWN;
            if (nameIsUnknown && typeIsUnknown) return null;

            if (!nameIsUnknown)
            {
                if (parent.TryLookupChild(childName) is not {} item) return null;
            
                if (typeIsUnknown || item.ItemType == (int) type)
                {
                    TcDte.TrackObject(item);
                    return item;
                }
            
                TcDte.ReleaseObject(item);
                return null;
            }
        
            foreach (ITcSmTreeItem item in parent)
            {
                TcDte.TrackObject(item);
                if (item.ItemType == (int)type) return item;
            }

            return null;
        }

        /// <summary>
        /// Searches for a child in the given <see cref="ITcSmTreeItem"/> by name.
        /// </summary>
        /// <param name="childName">The name of the child item.</param>
        /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? GetChild(string? childName) => 
            GetChild(parent, childName, TREEITEMTYPES.TREEITEMTYPE_UNKNOWN);

        /// <summary>
        /// Searches for a child in the given <see cref="ITcSmTreeItem"/> by type.
        /// </summary>
        /// <param name="type">The type of the child item.</param>
        /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? GetChild(TREEITEMTYPES type) => 
            GetChild(parent, null, type);

        /// <summary>
        /// Gets the child items as an enumeration of type <see cref="ITcSmTreeItem"/>.
        /// </summary>
        /// <returns>An enumeration of type <see cref="ITcSmTreeItem"/>, if any.</returns>
        public IEnumerable<ITcSmTreeItem> GetChildren()
        {
            if (parent is null) yield break;
            foreach (ITcSmTreeItem item in parent)
            {
                TcDte.TrackObject(item);
                yield return item;
            }
        }

        /// <summary>
        /// Tries to find a <see cref="ITcSmTreeItem"/> recursive.
        /// </summary>
        /// <param name="childName">The name of the child item.</param>
        /// <param name="type">The <see cref="TREEITEMTYPES"/> of the child item.</param>
        /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? GetChildRecursive(string? childName, TREEITEMTYPES type)
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
                    TcDte.ReleaseObject(child);
                    return grandChild;       
                }
                TcDte.ReleaseObject(child);
            }
        
            return null;
        }

        /// <summary>
        /// Gets a child in the given tree by name and type. Creates the child if not existent.
        /// </summary>
        /// <param name="childName">The name of the child item.</param>
        /// <param name="type">The <see cref="TREEITEMTYPES"/> of the child item.</param>
        /// <returns>The <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? GetOrCreateChild(string? childName, TREEITEMTYPES type)
        {
            var compatibleName = childName?.MakePlcCompatible();
            var item = parent.GetChild(compatibleName, type);
            if (item is not null) return item;
            var child = parent?.CreateChild(compatibleName, nSubType: (int)type);
            TcDte.TrackObject(child);
            return child;
        }

        /// <summary>
        /// Creates or overwrites a GVL item with the given content.
        /// </summary>
        /// <param name="name">The name of the GVL. 'GVL_' is automatically added at the start.</param>
        /// <param name="variables">The variable declarations.</param>
        /// <returns>The GVL <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? CreateGvl(string name, string? variables)
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
                $"{{attribute 'pack_mode' := '1'}}\n" +
                $"VAR_GLOBAL\n{variables}END_VAR";
        
            return gvlItem;
        }

        /// <summary>
        /// Creates or overwrites a DUT struct item with the given content.
        /// </summary>
        /// <param name="name">The name of the DUT. 'ST_' is automatically added at the start.</param>
        /// <param name="variables">The variable declarations.</param>
        /// <returns>The DUT struct <see cref="ITcSmTreeItem"/> if successful, otherwise null.</returns>
        public ITcSmTreeItem? CreateDutStruct(string name, string? variables)
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
}