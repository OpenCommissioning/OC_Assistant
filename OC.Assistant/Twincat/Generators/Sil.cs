using OC.Assistant.Core;
using OC.Assistant.Plugins;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat.Generators;

/// <summary>
/// Generator for SiL signals.
/// </summary>
internal static class Sil
{
    private const string FOLDER_NAME = nameof(Sil);
    
    /// <summary>
    /// Creates or removes a single SiL structure.
    /// </summary>
    /// <param name="plcProjectItem">The given plc project.</param>
    /// <param name="name">The name of the SiL plugin.</param>
    /// <param name="delete">False: Updates the SiL structure. True: Deletes the SiL structure.</param>
    public static void Update(ITcSmTreeItem plcProjectItem, string name, bool delete)
    {
        var sil = plcProjectItem.GetOrCreateChild(FOLDER_NAME, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (sil?.GetChild(name) is not null) sil.DeleteChild(name);
        if (delete)
        {
            if (sil?.ChildCount == 0) plcProjectItem.DeleteChild(sil.Name);
            return;
        }
        Generate(XmlFile.Instance.GetPlugin(name), plcProjectItem);
    }
    
    /// <summary>
    /// Updates all SiL structures.
    /// </summary>
    /// <param name="plcProjectItem">The given plc project.</param>
    public static void UpdateAll(ITcSmTreeItem plcProjectItem)
    {
        if (plcProjectItem.GetChild(FOLDER_NAME) is not null)
        {
            plcProjectItem.DeleteChild(FOLDER_NAME);
        }
        foreach (var plugin in XmlFile.Instance.PluginElements())
        {
            Generate(plugin, plcProjectItem);
        }
    }
    
    private static void Generate(XPlugin? plugin, ITcSmTreeItem plcProjectItem)
    {
        if (plugin is null) return;
        if (!Enum.TryParse(plugin.IoType, out IoType ioType) || ioType == IoType.None) return;
        
        if (plcProjectItem.GetOrCreateChild(FOLDER_NAME, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} silFolder)
        {
            return;
        }
        
        switch (ioType)
        {
            case IoType.Address:
                CreateAddressVariables(plugin, silFolder);
                break;
            case IoType.Struct:
                CreateStructVariables(plugin, silFolder);
                break;
        }
    }

    private static void CreateAddressVariables(XPlugin plugin, ITcSmTreeItem silFolder)
    {
        var name = plugin.Name;
        if (silFolder.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} folder)
        {
            return;
        } 
        
        var inputs = plugin.InputAddress?.Aggregate("", (current, next) => 
            $"{current}\tI{next}: {TcType.Byte.Name()};\n");
        
        var outputs = plugin.OutputAddress?.Aggregate("", (current, next) => 
            $"{current}\tQ{next}: {TcType.Byte.Name()};\n");

        folder.CreateGvl(name, inputs + outputs);
    }

    private static void CreateStructVariables(XPlugin plugin, ITcSmTreeItem silFolder)
    {
        var name = plugin.Name;
        if (silFolder.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} folder)
        {
            return;
        }
        
        var inputs = plugin.InputStructure.Elements().Aggregate("", (current, var) => 
            current + $"\t{var.Element("Name")?.Value}: {var.Element("Type")?.Value};\n");
        
        var outputs = plugin.OutputStructure.Elements().Aggregate("", (current, var) => 
            current + $"\t{var.Element("Name")?.Value}: {var.Element("Type")?.Value};\n");
        
        folder.CreateDutStruct($"{name}Inputs", inputs);
        folder.CreateDutStruct($"{name}Outputs", outputs);
        folder.CreateGvl(name, $"\tInputs : ST_{name}Inputs;\n\tOutputs : ST_{name}Outputs;\n");
    }
}