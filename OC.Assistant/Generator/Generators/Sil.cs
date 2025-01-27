using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
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
        Generate(XmlFile.PluginElements?.First(x => x.Attribute(XmlTags.PLUGIN_NAME)?.Value == name), plcProjectItem);
    }
    
    /// <summary>
    /// Updates all SiL structures.
    /// </summary>
    /// <param name="plcProjectItem">The given plc project.</param>
    public static void UpdateAll(ITcSmTreeItem plcProjectItem)
    {
        if (plcProjectItem.TryLookupChild(FOLDER_NAME) is not null) plcProjectItem.DeleteChild(FOLDER_NAME);
        if (XmlFile.PluginElements is null) return;
        foreach (var plugin in XmlFile.PluginElements)
        {
            Generate(plugin, plcProjectItem);
        }
    }
    
    private static void Generate(XElement? plugin, ITcSmTreeItem plcProjectItem)
    {
        if (plugin is null) return;
        if (!Enum.TryParse(plugin.Attribute(XmlTags.PLUGIN_IO_TYPE)?.Value, out IoType ioType)) return;
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

    private static string GvlDeclaration(string? variables)
    {
        return $"{{attribute 'qualified_only'}}\n{{attribute 'subsequent'}}\nVAR_GLOBAL\n{variables}END_VAR";
    }
    
    private static string DutDeclaration(string name, string? variables)
    {
        return $"{{attribute 'pack_mode' := '0'}}\nTYPE ST_{name} :\nSTRUCT\n{variables}END_STRUCT\nEND_TYPE"; 
    }

    private static void AddressVariables(XElement plugin, ITcSmTreeItem silFolder)
    {
        var pluginName = plugin.Attribute(XmlTags.PLUGIN_NAME)?.Value;
        if (pluginName is null) return;
        
        var pluginFolder = silFolder.GetOrCreateChild(pluginName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (pluginFolder?.GetOrCreateChild($"GVL_{pluginName}", TREEITEMTYPES.TREEITEMTYPE_PLCGVL) is not ITcPlcDeclaration gvlDecl) return;
        
        var gvlVariables = "";
        
        //Inputs
        var request = plugin
            .Element(XmlTags.PLUGIN_PARAMETER)?
            .Element(XmlTags.PLUGIN_PARAMETER_INPUT_ADDRESS)?.Value.ToNumberList();
        
        if (request is not null)
        {
            gvlVariables = request.Aggregate(gvlVariables, (current, t) => 
                current + $"\tI{t}: {TcType.Byte.Name()};\n");
        }
                    
        //Outputs
        request = plugin
            .Element(XmlTags.PLUGIN_PARAMETER)?
            .Element(XmlTags.PLUGIN_PARAMETER_OUTPUT_ADDRESS)?.Value.ToNumberList();
        
        if (request is null) return;
        gvlVariables = request.Aggregate(gvlVariables, (current, t) => 
            current + $"\tQ{t}: {TcType.Byte.Name()};\n");

        gvlDecl.DeclarationText = GvlDeclaration(gvlVariables);
    }

    private static void StructVariables(XElement plugin, ITcSmTreeItem silFolder)
    {
        var pluginName = plugin.Attribute(XmlTags.PLUGIN_NAME)?.Value;
        if (pluginName is null) return;
        
        var pluginFolder = silFolder.GetOrCreateChild(pluginName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        if (pluginFolder?.GetOrCreateChild($"GVL_{pluginName}", TREEITEMTYPES.TREEITEMTYPE_PLCGVL) is not ITcPlcDeclaration gvlDecl) return;
        if (pluginFolder.GetOrCreateChild($"ST_{pluginName}Inputs", TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not ITcPlcDeclaration inputDut) return;
        if (pluginFolder.GetOrCreateChild($"ST_{pluginName}Outputs", TREEITEMTYPES.TREEITEMTYPE_PLCDUTSTRUCT) is not ITcPlcDeclaration outputDut) return;
        
        var inputStruct = plugin.Element(XmlTags.PLUGIN_INPUT_STRUCT);
        var outputStruct = plugin.Element(XmlTags.PLUGIN_OUTPUT_STRUCT);
        
        //Inputs
        var dutVariables = "";
        dutVariables = inputStruct?.Elements()
            .Aggregate(dutVariables, (current, var) => 
                current + $"\t{var.Element(XmlTags.PLUGIN_NAME)?.Value}: {var.Element(XmlTags.PLUGIN_TYPE)?.Value};\n");
        inputDut.DeclarationText = DutDeclaration($"{pluginName}Inputs", dutVariables);
                    
        //Output
        dutVariables = "";
        dutVariables = outputStruct?.Elements()
            .Aggregate(dutVariables, (current, var) => 
                current + $"\t{var.Element(XmlTags.PLUGIN_NAME)?.Value}: {var.Element(XmlTags.PLUGIN_TYPE)?.Value};\n");
        outputDut.DeclarationText = DutDeclaration($"{pluginName}Outputs", dutVariables);
        
        var gvlVariables = 
            $"\tInputs : ST_{pluginName}Inputs;\n" +
            $"\tOutputs : ST_{pluginName}Outputs;\n";
        gvlDecl.DeclarationText = GvlDeclaration(gvlVariables);
    }
}