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

            var instances = new List<PouInstanceCall>();
            var main = XmlFile.Main;
            if (main is null) return;

            //Iterate elements under main
            foreach (var child in main.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "Device":
                        instances.Add(new PouInstanceCall(child));
                        continue;
                    case "Group":
                        instances.Add(new PouInstanceCall(child));
                        CreateGroup(plcProjectItem, child);
                        continue;
                }
            }
            
            instances.Insert(0, new PouInstanceCall("fbSystem", "FB_System"));
            
            //HiL calls
            var additionalCycleText = XmlFile.HilPrograms?.
                Aggregate("", (current, next) => $"{current}\tPRG_{next}();\r\n");
            
            SetPouContent(mainPrg, instances, additionalCycleText);
        });
    }
    
    private static void CreateGroup(ITcSmTreeItem? parent, XElement? group, string? parentName = "")
    {
        Retry.Invoke(() =>
        {
            if (group is null) return;
            var instances = new List<PouInstanceCall>();
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
                        instances.Add(new PouInstanceCall(child));
                        continue;
                    case "Group":
                        instances.Add(new PouInstanceCall(
                            child.Attribute("Name")?.Value, 
                            $"{fbName}_{child.Attribute("Name")?.Value}"));
                        CreateGroup(folder, child, fbName);
                        continue;
                }
            }

            CreateFb(folder, fbName, instances);
        });
    }
    
    private static void CreateFb(ITcSmTreeItem? parent, string? name, IReadOnlyCollection<PouInstanceCall> instances)
    {
        Retry.Invoke(() =>
        {
            SetPouContent(parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB), instances);
        });
    }
    
    private static void SetPouContent(ITcSmTreeItem? pou, IReadOnlyCollection<PouInstanceCall> instances, string? additionalCycleText = null)
    {
        if (pou is null) return;
        
        Retry.Invoke(() =>
        {
            //Get and clean up implementation and declaration
            if (pou is not ITcPlcDeclaration decl) return;
            if (pou is not ITcPlcImplementation impl) return;
            impl.ImplementationText = Cleanup(impl.ImplementationText);
            decl.DeclarationText = Cleanup(decl.DeclarationText);
            
            //Declaration
            SetDeclaration(decl, instances.Aggregate("", (current, next) => $"{current}{next.DeclarationText}"));
            
            //InitRun action
            var initRun = CreatePouAction(pou, "InitRun");
            var initRunText = "\tIF NOT bInitRun THEN RETURN; END_IF\r\n\tbInitRun := FALSE;\r\n";
            initRunText = instances
                .Aggregate(initRunText, (current, next) => $"{current}{next.InitRunText}");
            SetImplementation(initRun, initRunText);
            
            //Cycle action
            var cycle = CreatePouAction(pou, "Cycle");
            var cycleText = instances
                .Aggregate("", (current, next) => $"{current}{next.ImplementationText}");
            SetImplementation(cycle, cycleText + $"{additionalCycleText}");

            //Implementation
            SetImplementation(impl, "\tInitRun();\r\n\tCycle();\r\n");
        });
    }
    
    private static ITcPlcImplementation? CreatePouAction(ITcSmTreeItem? parent, string name)
    {
        return Retry.Invoke(() =>
        {
            var action = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCACTION);
            if (action is not ITcPlcImplementation implementation) return null;
            implementation.ImplementationText = Cleanup(implementation.ImplementationText);
            return implementation;
        });
    }
    
    private static void SetImplementation(ITcPlcImplementation? impl, string? text)
    {
        if (impl is null || string.IsNullOrEmpty(text)) return;
        
        Retry.Invoke(() =>
        {
            var existingText = impl.ImplementationText;
            var generatedText = "{region generated code}\r\n";
            generatedText += text;
            generatedText += "{endregion}\r\n";
            generatedText += existingText;
            impl.ImplementationText = generatedText;
        });
    }
    
    private static void SetDeclaration(ITcPlcDeclaration? decl, string? text)
    {
        if (decl is null || string.IsNullOrEmpty(text)) return;
        
        Retry.Invoke(() =>
        {
            var generatedText = "\r\n{region generated code}\r\nVAR_INPUT\r\n";
            generatedText += $"\tbInitRun : BOOL := TRUE;\r\n{text}";
            generatedText += "END_VAR\r\n{endregion}";
            decl.DeclarationText += generatedText;
        });
    }
    
    private static string Cleanup(string input)
    {
        var result = Regex
            .Replace(input, @"\s*\{region generated code\}.*?\{endregion\}\s*", "\r\n", RegexOptions.Singleline)
            .Replace("VAR_INPUT\r\nEND_VAR\r\n", "")
            .Replace("VAR_OUTPUT\r\nEND_VAR\r\n", "")
            .Replace("VAR\r\nEND_VAR\r\n", "");
        return result == "\r\n" ? "" : result;
    }
}