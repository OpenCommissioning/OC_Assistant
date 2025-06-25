using System.Diagnostics.CodeAnalysis;
using OC.Assistant.Core;
using OC.Assistant.Plugins;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Generators;

/// <summary>
/// Generator for SiL signals.
/// </summary>
[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
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
        if (sil?.TryLookupChild(name) is not null) sil.DeleteChild(name);
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
        if (plcProjectItem.TryLookupChild(FOLDER_NAME) is not null) plcProjectItem.DeleteChild(FOLDER_NAME);
        foreach (var plugin in XmlFile.Instance.PluginElements())
        {
            Generate(plugin, plcProjectItem);
        }
    }
    
    private static void Generate(XPlugin? plugin, ITcSmTreeItem plcProjectItem)
    {
        if (plugin is null) return;
        if (!Enum.TryParse(plugin.IoType, out IoType ioType)) return;
        if (ioType == IoType.None) return;
        var silFolder = plcProjectItem.GetOrCreateChild(FOLDER_NAME, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (silFolder is null) return;
        
        switch (ioType)
        {
            case IoType.Address:
                AddressVariables(plugin, silFolder);
                break;
            case IoType.Struct:
                StructVariables(plugin, silFolder);
                break;
        }
    }

    private static void AddressVariables(XPlugin plugin, ITcSmTreeItem silFolder)
    {
        var pluginName = plugin.Name;
        
        var pluginFolder = silFolder.GetOrCreateChild(pluginName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (pluginFolder is null) return;
        
        var gvlVariables = "";
        
        //Inputs
        var request = plugin.InputAddress;
        
        if (request is not null)
        {
            gvlVariables = request.Aggregate(gvlVariables, (current, t) => 
                current + $"\tI{t}: {TcType.Byte.Name()};\n");
        }
                    
        //Outputs
        request = plugin.OutputAddress;
        
        if (request is null) return;
        gvlVariables = request.Aggregate(gvlVariables, (current, t) => 
            current + $"\tQ{t}: {TcType.Byte.Name()};\n");

        pluginFolder.CreateGvl(pluginName, gvlVariables);
    }

    private static void StructVariables(XPlugin plugin, ITcSmTreeItem silFolder)
    {
        var pluginName = plugin.Name;
        
        var pluginFolder = silFolder.GetOrCreateChild(pluginName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (pluginFolder is null) return;
        
        //Input struct
        var variables = "";
        variables = plugin.InputStructure.Elements()
            .Aggregate(variables, (current, var) => 
                current + $"\t{var.Element("Name")?.Value}: {var.Element("Type")?.Value};\n");
        pluginFolder.CreateDutStruct($"{pluginName}Inputs", variables);
                    
        //Output struct
        variables = "";
        variables = plugin.OutputStructure.Elements()
            .Aggregate(variables, (current, var) => 
                current + $"\t{var.Element("Name")?.Value}: {var.Element("Type")?.Value};\n");
        pluginFolder.CreateDutStruct($"{pluginName}Outputs", variables);
        
        //GVL
        pluginFolder.CreateGvl(pluginName, 
            $"\tInputs : ST_{pluginName}Inputs;\n\tOutputs : ST_{pluginName}Outputs;\n");
    }
}