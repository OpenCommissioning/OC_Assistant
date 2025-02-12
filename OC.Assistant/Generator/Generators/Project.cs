using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
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
        var main = XmlFile.Main;
        if (main is null) return;
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
        var name = group.Attribute("Name")?.Value.TcPlcCompatibleString();
        var fbName = parentName is null ? name : $"{parentName}_{name}".TcPlcCompatibleString();
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
                    instances.Add(new PouInstance(childName, $"{fbName}_{childName}"));
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
        
        //Get implementation and declaration
        if (pou is not ITcPlcDeclaration decl) return;
        if (pou is not ITcPlcImplementation impl) return;
        
        //Set declaration
        const string declaration = "\tbInitRun : BOOL := TRUE;\n\tfbSystem : FB_System;\n";
        SetDeclaration(decl, instances
            .Aggregate(declaration, (current, next) => $"{current}{next.DeclarationText}"));
        
        //Create or set InitRun method
        CreateInitRun(pou, instances, true);
        
        //InitRun() and fbSystem()
        var implementation = "\tInitRun();\n\tfbSystem();\n";
        
        //HiL calls
        implementation = XmlFile.HilPrograms?.
            Aggregate(implementation, (current, next) => $"{current}PRG_{next}();\n");

        //Instance calls
        implementation = instances
            .Aggregate(implementation, (current, next) => $"{current}{next.ImplementationText}");
        
        //Set implementation
        SetImplementation(impl, implementation);
    }
    
    private static void CreateFb(ITcSmTreeItem? parent, string? name, IReadOnlyCollection<PouInstance> instances)
    {
        var pou = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB);
        
        //Get implementation and declaration
        if (pou is not ITcPlcDeclaration decl) return;
        if (pou is not ITcPlcImplementation impl) return;
        
        //Set declaration
        SetDeclaration(decl, instances
            .Aggregate("", (current, next) => $"{current}{next.DeclarationText}"));
        
        //Create or set InitRun method
        CreateInitRun(pou, instances);

        //Set implementation
        var cycleText = instances
            .Aggregate("\tInitRun();\n", (current, next) => $"{current}{next.ImplementationText}");
        SetImplementation(impl, cycleText);
    }
    
    private static void CreateInitRun(ITcSmTreeItem? parent, IReadOnlyCollection<PouInstance> instances, bool isProgram = false)
    {
        //Create method 'InitRun'
        var method = parent?.GetOrCreateChild("InitRun", TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD);
        
        //Set declaration
        if (!isProgram && method is ITcPlcDeclaration decl)
        {
            SetDeclaration(decl, "\tbInitRun : BOOL := TRUE;\n", isMethod: true);
        }
        
        //Set implementation
        if (method is not ITcPlcImplementation impl) return;
        var text = instances.Aggregate("\tIF NOT bInitRun THEN RETURN; END_IF\n\tbInitRun := FALSE;\n", 
            (current, next) => $"{current}{next.InitRunText}");
        SetImplementation(impl, text);
    }
    
    private static void SetImplementation(ITcPlcImplementation? impl, string? text)
    {
        if (impl is null || string.IsNullOrEmpty(text)) return;
        
        var existing = impl.ImplementationText;
        var customText = Cleanup(existing);
        var generatedText = "{region generated code}\n";
        generatedText += text;
        generatedText += "{endregion}\n";
        generatedText += customText;
        
        if (IsEqual(generatedText, existing))
        {
            return;
        }
        
        impl.ImplementationText = generatedText;
    }
    
    private static void SetDeclaration(ITcPlcDeclaration? decl, string? text, bool isMethod = false)
    {
        if (decl is null || string.IsNullOrEmpty(text)) return;
        
        var existing = decl.DeclarationText;
        var customText = Cleanup(existing);
        var generatedText = "\n{region generated code}\n";
        generatedText += isMethod ? "VAR_INST\n" : "VAR_INPUT\n";
        generatedText += text;
        generatedText += "END_VAR\n{endregion}";
        generatedText = customText + generatedText;
        
        if (IsEqual(generatedText, existing))
        {
            return;
        }
        
        decl.DeclarationText = generatedText;
    }
    
    [GeneratedRegex(@"\s*\{region generated code\}.*?\{endregion\}\s*", RegexOptions.Singleline)]
    private static partial Regex RegionPattern();
    
    private static string Cleanup(string input)
    {
        var result = RegionPattern().Replace(input, "\n")
            .Replace("VAR_INPUT\nEND_VAR\n", "")
            .Replace("VAR_OUTPUT\nEND_VAR\n", "")
            .Replace("VAR\nEND_VAR\n", "");
        return result == "\n" ? "" : result;
    }

    private static bool IsEqual(string str1, string str2)
    {
        return string.Equals(
            str1.Replace("\r", ""), 
            str2.Replace("\r", ""), 
            StringComparison.OrdinalIgnoreCase
        );
    }
}