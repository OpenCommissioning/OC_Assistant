using System.IO;
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
internal static class Sil
{
    private const string FOLDER_NAME = nameof(Sil);
    private const string VAR_DECL = "$VARNAME$: $VARTYPE$;\n";
    
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
        GenerateFiles(XmlFile.PluginElements?.First(x => x.Attribute(XmlTags.PLUGIN_NAME)?.Value == name));
        plcProjectItem.TcIntegrate(FOLDER_NAME);
    }
    
    /// <summary>
    /// Updates all SiL structures.
    /// </summary>
    /// <param name="plcProjectItem">The given plc project.</param>
    public static void UpdateAll(ITcSmTreeItem plcProjectItem)
    {
        if (plcProjectItem.TryLookupChild(FOLDER_NAME) is not null) plcProjectItem.DeleteChild(FOLDER_NAME);
        if (XmlFile.PluginElements is null) return;
        foreach (var plugin in XmlFile.PluginElements) GenerateFiles(plugin);
        plcProjectItem.TcIntegrate(FOLDER_NAME);
    }
    
    private static void GenerateFiles(XElement? plugin)
    {
        if (plugin == default) return;
        if (!Enum.TryParse(plugin.Attribute(XmlTags.PLUGIN_IO_TYPE)?.Value, out IoType ioType)) return;
        if (ioType == IoType.None) return;
            
        var pluginName = plugin.Attribute(XmlTags.PLUGIN_NAME)?.Value;
            
        //Create folder if not exists already
        Directory.CreateDirectory($@"{AppData.Path}\{FOLDER_NAME}\{pluginName}");

        if (pluginName is null) return;
        
        switch (ioType)
        {
            case IoType.Address:
                AddressVariables(plugin).CreateGvl(FOLDER_NAME, pluginName);
                break;
            case IoType.Struct:
                StructVariables(plugin).CreateGvl(FOLDER_NAME, pluginName);
                break;
        }
    }

    private static string AddressVariables(XContainer plugin)
    {
        var gvlVariables = "";
        
        //Inputs
        var request = plugin
            .Element(XmlTags.PLUGIN_PARAMETER)?
            .Element(XmlTags.PLUGIN_PARAMETER_INPUT_ADDRESS)?.Value.ToNumberList();
        
        if (request is not null)
        {
            gvlVariables = request.Aggregate(gvlVariables, (current, t) => current + $"\t{VAR_DECL}"
                .Replace(Tags.VAR_NAME, $"I{t}")
                .Replace(Tags.VAR_TYPE, TcType.Byte.Name()));
        }
                    
        //Outputs
        request = plugin
            .Element(XmlTags.PLUGIN_PARAMETER)?
            .Element(XmlTags.PLUGIN_PARAMETER_OUTPUT_ADDRESS)?.Value.ToNumberList();
        
        if (request is null) return gvlVariables;
        gvlVariables = request.Aggregate(gvlVariables, (current, t) => current + $"\t{VAR_DECL}"
            .Replace(Tags.VAR_NAME, $"Q{t}")
            .Replace(Tags.VAR_TYPE, TcType.Byte.Name()));

        return gvlVariables;
    }

    private static string StructVariables(XElement plugin)
    {
        var pluginName = plugin.Attribute(XmlTags.PLUGIN_NAME)?.Value;
        var inputStruct = plugin.Element(XmlTags.PLUGIN_INPUT_STRUCT);
        var outputStruct = plugin.Element(XmlTags.PLUGIN_OUTPUT_STRUCT);

        if (pluginName is null) return "";
                      
        //Inputs
        var dutVariables = "";
        dutVariables = inputStruct?.Elements()
            .Aggregate(dutVariables, (current, var) => current + $"\t{VAR_DECL}"
                .Replace(Tags.VAR_NAME, var.Element(XmlTags.PLUGIN_NAME)?.Value)
                .Replace(Tags.VAR_TYPE, var.Element(XmlTags.PLUGIN_TYPE)?.Value));
        var type = dutVariables?.CreateDut(FOLDER_NAME, pluginName, "Inputs");
        
        var gvlVariables = $"\t{VAR_DECL}"
            .Replace(Tags.VAR_NAME, "Inputs")
            .Replace(Tags.VAR_TYPE, type);
                    
        //Output
        dutVariables = "";
        dutVariables = outputStruct?.Elements()
            .Aggregate(dutVariables, (current, var) => current + $"\t{VAR_DECL}"
                .Replace(Tags.VAR_NAME, var.Element(XmlTags.PLUGIN_NAME)?.Value)
                .Replace(Tags.VAR_TYPE, var.Element(XmlTags.PLUGIN_TYPE)?.Value));
        type = dutVariables?.CreateDut(FOLDER_NAME, pluginName, "Outputs");
        
        gvlVariables += $"\t{VAR_DECL}"
            .Replace(Tags.VAR_NAME, "Outputs")
            .Replace(Tags.VAR_TYPE, type);

        return gvlVariables;
    }
}