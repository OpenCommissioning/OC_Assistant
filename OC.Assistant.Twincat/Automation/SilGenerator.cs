using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat.Automation;

/// <summary>
/// Generator for SiL signals.
/// </summary>
internal static class SilGenerator
{
    private const string FOLDER_NAME = "Sil";
    
    /// <summary>
    /// Creates a single SiL structure.
    /// </summary>
    /// <param name="plcProjectItem">The given plc project.</param>
    /// <param name="plugin">The SiL plugin.</param>
    public static void Update(ITcSmTreeItem plcProjectItem, XPlugin? plugin)
    {
        if (plugin is null) return;
        var sil = plcProjectItem.GetOrCreateChild(FOLDER_NAME, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (sil?.GetChild(plugin.Name) is not null) sil.DeleteChild(plugin.Name);
        Generate(plugin, plcProjectItem);
    }

    /// <summary>
    /// Removes a single SiL structure.
    /// </summary>
    /// <param name="plcProjectItem">The given plc project.</param>
    /// <param name="pluginName">The plugin name.</param>
    public static void Remove(ITcSmTreeItem plcProjectItem, string? pluginName)
    {
        if (string.IsNullOrEmpty(pluginName)) return;
        var sil = plcProjectItem.GetOrCreateChild(FOLDER_NAME, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (sil?.GetChild(pluginName) is not null) sil.DeleteChild(pluginName);
        if (sil?.ChildCount == 0) plcProjectItem.DeleteChild(sil.Name);
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
        foreach (var plugin in XmlFile.Instance.GetPluginElements())
        {
            Generate(plugin, plcProjectItem);
        }
    }
    
    private static void Generate(XPlugin? plugin, ITcSmTreeItem plcProjectItem)
    {
        if (plugin is null || plugin.ChannelType != nameof(TcAdsChannel)) return;
        
        var inputs = plugin.InputStructure.Elements().Aggregate("", (current, var) => 
            current + $"\t{var.Element("Name")?.Value.TcPlcCompatibleString()}: {var.Element("Type")?.Value};\n");
        
        var outputs = plugin.OutputStructure.Elements().Aggregate("", (current, var) => 
            current + $"\t{var.Element("Name")?.Value.TcPlcCompatibleString()}: {var.Element("Type")?.Value};\n");
        
        if (string.IsNullOrEmpty(inputs) && string.IsNullOrEmpty(outputs)) return;
        
        if (plcProjectItem.GetOrCreateChild(FOLDER_NAME, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} silFolder)
        {
            return;
        }
        
        var name = plugin.Name;
        if (silFolder.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER) is not {} folder)
        {
            return;
        }
        
        folder.CreateDutStruct($"{name}Inputs", inputs);
        folder.CreateDutStruct($"{name}Outputs", outputs);
        folder.CreateGvl(name, $"\tInputs : ST_{name}Inputs;\n\tOutputs : ST_{name}Outputs;\n");
    }
}