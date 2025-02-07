using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Core.TwinCat;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Generators;

/// <summary>
/// Generator for the plc project.
/// </summary>
[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
internal static class Project
{
    /// <summary>
    /// Updates the given plc project.
    /// </summary>
    public static void Update(ITcSmTreeItem plcProjectItem)
    {
        //Search main program
        var mainPrg = plcProjectItem.FindChildRecursive("main", TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG);
        if (mainPrg is null)
        {
            Logger.LogError(typeof(Project), "Main program not found");
            return;
        }

        var instances = new List<PouInstance>{ new("fbSystem", "FB_System") };
        var main = XmlFile.Main;
        if (main is null) return;

        //Iterate elements under main
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
        
        //HiL calls
        var additionalImplementation = XmlFile.HilPrograms?.
            Aggregate("", (current, next) => $"{current}PRG_{next}();\n");
        
        SetPouContent(mainPrg, instances, additionalImplementation);
    }
    
    private static void CreateGroup(ITcSmTreeItem? parent, XElement? group, string? parentName = "")
    {
        if (group is null) return;
        var name = group.Attribute("Name")?.Value;
        name = name?.TcPlcCompatibleString();
        var fbName = parentName == "" ? name : $"{parentName}_{name}";
        fbName = fbName?.TcPlcCompatibleString();
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
    
    private static void CreateFb(ITcSmTreeItem? parent, string? name, IReadOnlyCollection<PouInstance> instances)
    {
        SetPouContent(parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB), instances);
    }
    
    private static void SetPouContent(ITcSmTreeItem? pou, IReadOnlyCollection<PouInstance> instances, string? additionalImplementation = null)
    {
        if (pou is null) return;
        
        //Get implementation and declaration
        if (pou is not ITcPlcDeclaration decl) return;
        if (pou is not ITcPlcImplementation impl) return;
        
        //Set declaration
        SetDeclaration(decl, instances.Aggregate("", (current, next) => $"{current}{next.DeclarationText}"));
        
        //Create or set InitRun method
        SetInitRun(pou, instances);

        //Set implementation
        var cycleText = instances
            .Aggregate("\tInitRun();\n", (current, next) => $"{current}{next.ImplementationText}");
        SetImplementation(impl, cycleText + additionalImplementation);
    }
    
    private static void SetInitRun(ITcSmTreeItem? parent, IReadOnlyCollection<PouInstance> instances)
    {
        //Create method 'InitRun'
        var method = parent?.GetOrCreateChild("InitRun", TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD);
        
        //Get implementation and declaration
        if (method is not ITcPlcDeclaration decl) return;
        if (method is not ITcPlcImplementation impl) return;
        
        //Set declaration
        SetDeclaration(decl, "\tbInitRun : BOOL := TRUE;\n", isMethod: true);
        
        //Set implementation
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
    
    private static string Cleanup(string input)
    {
        var result = Regex
            .Replace(input, @"\s*\{region generated code\}.*?\{endregion\}\s*", "\n", RegexOptions.Singleline)
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