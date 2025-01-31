using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Core;
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
        Retry.Invoke(() =>
        {
            //Search MAIN program
            if (!plcProjectItem.FindMain(out var mainPrg))
            {
                Logger.LogError(typeof(Project), "MAIN program not found");
                return;
            }

            var instances = new List<PouInstance>();
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
            
            instances.Insert(0, new PouInstance("fbSystem", "FB_System"));
            
            //HiL calls
            var additionalCycleText = XmlFile.HilPrograms?.
                Aggregate("", (current, next) => $"{current}PRG_{next}();\n");
            
            SetPouContent(mainPrg, instances, additionalCycleText);
        });
    }
    
    private static void CreateGroup(ITcSmTreeItem? parent, XElement? group, string? parentName = "")
    {
        Retry.Invoke(() =>
        {
            if (group is null) return;
            var instances = new List<PouInstance>();
            var name = group.Attribute("Name")?.Value;
            name = name?.TcPlcCompatibleString();
            var fbName = parentName == "" ? name : $"{parentName}_{name}";
            fbName = fbName?.TcPlcCompatibleString();
            var folder = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);

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
        });
    }
    
    private static void CreateFb(ITcSmTreeItem? parent, string? name, IReadOnlyCollection<PouInstance> instances)
    {
        Retry.Invoke(() =>
        {
            SetPouContent(parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB), instances);
        });
    }
    
    private static void SetPouContent(ITcSmTreeItem? pou, IReadOnlyCollection<PouInstance> instances, string? additionalCycleText = null)
    {
        if (pou is null) return;
        
        Retry.Invoke(() =>
        {
            //Get implementation and declaration
            if (pou is not ITcPlcDeclaration decl) return;
            if (pou is not ITcPlcImplementation impl) return;
            
            //Set Declaration
            SetDeclaration(decl, instances.Aggregate("", (current, next) => $"{current}{next.DeclarationText}"));
            
            //InitRun action
            var initRun = CreatePouAction(pou, "InitRun");
            var initRunText = "\tIF NOT bInitRun THEN RETURN; END_IF\n\tbInitRun := FALSE;\n";
            initRunText = instances
                .Aggregate(initRunText, (current, next) => $"{current}{next.InitRunText}");
            SetImplementation(initRun, initRunText);
            
            //Cycle action
            var cycle = CreatePouAction(pou, "Cycle");
            var cycleText = instances
                .Aggregate("", (current, next) => $"{current}{next.ImplementationText}");
            SetImplementation(cycle, cycleText + $"{additionalCycleText}");

            //Set Implementation
            SetImplementation(impl, "\tInitRun();\n\tCycle();\n");
        });
    }
    
    private static ITcPlcImplementation? CreatePouAction(ITcSmTreeItem? parent, string name)
    {
        return Retry.Invoke(() =>
        {
            var action = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCACTION);
            return action as ITcPlcImplementation;
        });
    }
    
    private static void SetImplementation(ITcPlcImplementation? impl, string? text)
    {
        if (impl is null || string.IsNullOrEmpty(text)) return;
        
        Retry.Invoke(() =>
        {
            var existing = impl.ImplementationText;
            var customText = Cleanup(impl.ImplementationText);
            var generatedText = "{region generated code}\n";
            generatedText += text;
            generatedText += "{endregion}\n";
            generatedText += customText;
            
            
            if (IsEqual(generatedText, existing))
            {
                return;
            }
            
            impl.ImplementationText = generatedText;
        });
    }
    
    private static void SetDeclaration(ITcPlcDeclaration? decl, string? text)
    {
        if (decl is null || string.IsNullOrEmpty(text)) return;
        
        Retry.Invoke(() =>
        {
            var existing = decl.DeclarationText;
            var customText = Cleanup(decl.DeclarationText);
            var generatedText = "\n{region generated code}\nVAR_INPUT\n";
            generatedText += $"\tbInitRun : BOOL := TRUE;\n{text}";
            generatedText += "END_VAR\n{endregion}";
            generatedText = customText + generatedText;
            
            if (IsEqual(generatedText, existing))
            {
                return;
            }
            
            decl.DeclarationText = generatedText;
        });
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