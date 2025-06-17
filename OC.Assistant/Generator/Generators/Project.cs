using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Core;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Generators;

/// <summary>
/// Generator for the plc project.
/// </summary>
[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
internal static partial class Project
{
    /// <summary>
    /// Updates the given plc project.
    /// </summary>
    public static void Update(ITcSmTreeItem plcProjectItem)
    {
        var main = XmlFile.Instance.ProjectMain;
        var instances = new List<PouInstance>();
        
        foreach (var child in main.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "Device":
                    instances.Add(new PouInstance(child));
                    continue;
                case "Group":
                    var childName = child.Attribute("Name")?.Value;
                    instances.Add(new PouInstance(childName, childName));
                    CreateGroup(plcProjectItem, child);
                    continue;
            }
        }
        
        CreateMainPrg(plcProjectItem, instances);
    }
    
    private static void CreateGroup(ITcSmTreeItem? parent, XElement? group, string? parentName = null)
    {
        if (group is null) return;
        var name = group.Attribute("Name")?.Value;
        var fbName = parentName is null ? name : $"{parentName}{name}";
        var folder = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        var instances = new List<PouInstance>();
        
        foreach (var child in group.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "Device":
                    instances.Add(new PouInstance(child));
                    continue;
                case "Group":
                    var childName = child.Attribute("Name")?.Value;
                    instances.Add(new PouInstance(childName, $"{fbName}{childName}"));
                    CreateGroup(folder, child, fbName);
                    continue;
            }
        }
        
        CreateFb(folder, fbName, instances);
    }
    
    private static void CreateMainPrg(ITcSmTreeItem? parent, IReadOnlyCollection<PouInstance> instances)
    {
        //Find or create main program
        var pou = parent?.FindChildRecursive("main", TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG);
        pou ??= parent?.GetOrCreateChild("MAIN", TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG);
        
        //Declaration
        const string declaration = "\tbInitRun : BOOL := TRUE;\n\tfbSystem : FB_System;\n";
        pou.UpdateDeclaration(instances
            .Aggregate(declaration, (current, next) => $"{current}{next.DeclarationText}"));
        
        //Create or set InitRun method
        CreateInitRun(pou, instances, true);
        
        //InitRun() and fbSystem()
        var implementation = "\tInitRun();\n\tfbSystem();\n";
        
        //HiL calls
        implementation = XmlFile.Instance.ProjectHil.Elements().Select(x => x.Value).
            Aggregate(implementation, (current, next) => $"{current}\t{next}();\n");

        //Instance calls
        implementation = instances
            .Aggregate(implementation, (current, next) => $"{current}{next.ImplementationText}");
        
        //Implementation
        pou.UpdateImplementation(implementation);
    }
    
    private static void CreateFb(ITcSmTreeItem? parent, string? name, IReadOnlyCollection<PouInstance> instances)
    {
        var pou = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB);
        
        //Declaration
        pou.UpdateDeclaration(instances
            .Aggregate("", (current, next) => $"{current}{next.DeclarationText}"));
        
        //Create or set InitRun method
        CreateInitRun(pou, instances);

        //Implementation
        pou.UpdateImplementation(instances
            .Aggregate("\tInitRun();\n", (current, next) => $"{current}{next.ImplementationText}"));
    }
    
    private static void CreateInitRun(ITcSmTreeItem? parent, IReadOnlyCollection<PouInstance> instances, bool isProgram = false)
    {
        //Create method 'InitRun'
        var method = parent?.GetOrCreateChild("InitRun", TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD);
        
        //Set declaration
        if (!isProgram)
        {
            method?.UpdateDeclaration("\tbInitRun : BOOL := TRUE;\n");
        }
        
        //Set implementation
        var text = instances.Aggregate("\tIF NOT bInitRun THEN RETURN; END_IF\n\tbInitRun := FALSE;\n", 
            (current, next) => $"{current}{next.InitRunText}");
        method.UpdateImplementation(text);
    }

    private static void UpdateDeclaration(this ITcSmTreeItem? item, string? text)
    {
        if (item is not ITcPlcDeclaration declaration) return;
        ReplaceGeneratedText(declaration, text, item.ItemType == (int) TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD);
    }
    
    private static void UpdateImplementation(this ITcSmTreeItem? item, string? text)
    {
        if (item is not ITcPlcImplementation implementation) return;
        ReplaceGeneratedText(implementation, text);
    }
    
    private static void ReplaceGeneratedText(ITcPlcImplementation? impl, string? text)
    {
        if (impl is null || string.IsNullOrEmpty(text)) return;
        
        var existingText = impl.ImplementationText;
        var generatedText = $"{{region generated code}}\n{text}{{endregion}}";

        if (GetGeneratedText(existingText) == generatedText) return;
        impl.ImplementationText = $"{generatedText}\n{RemoveGeneratedText(existingText)}";
    }
    
    private static void ReplaceGeneratedText(ITcPlcDeclaration? decl, string? text, bool isMethod = false)
    {
        if (decl is null || string.IsNullOrEmpty(text)) return;
        
        var existingText = decl.DeclarationText;
        var varType = isMethod ? "VAR_INST" : "VAR_INPUT";
        var generatedText = $"{{region generated code}}\n{varType}\n{text}END_VAR\n{{endregion}}";
        
        if (GetGeneratedText(existingText) == generatedText) return;
        decl.DeclarationText = $"{RemoveGeneratedText(existingText)}\n{generatedText}";
    }
    
    [GeneratedRegex(@"\s*\{region generated code\}.*?\{endregion\}\s*", RegexOptions.Singleline)]
    private static partial Regex PatternRemoveGenerated();
    
    [GeneratedRegex(@"{region generated code\}.*?\{endregion\}", RegexOptions.Singleline)]
    private static partial Regex PatternGetGenerated();

    private static string GetGeneratedText(string input)
    {
        return PatternGetGenerated().Match(input).Value;
    }
    
    private static string RemoveGeneratedText(string input)
    {
        var result = PatternRemoveGenerated().Replace(input, "\n")
            //also remove empty variable declarations
            .Replace("\nVAR_INPUT\nEND_VAR", "")
            .Replace("\nVAR_OUTPUT\nEND_VAR", "")
            .Replace("\nVAR\nEND_VAR", "");
        return result == "\n" ? "" : result;
    }
}