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
    private const string START_REGION = "{region generated code}";
    private const string END_REGION = "{endregion}";
    
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
                //element is device -> direct instance
                if (child.Name == "Device")
                {
                    instances.Add(new PouInstanceCall(
                        name: child.Attribute("Name")?.Value.TcPlcCompatibleString(), 
                        type: child.Attribute("Type")?.Value, 
                        comment: child.Element("Comment")?.Value));
                }

                //element is group -> create group and instance call
                if (child.Name != "Group") continue;
                instances.Add(new PouInstanceCall(
                    name: child.Attribute("Name")?.Value.TcPlcCompatibleString(), 
                    type: child.Attribute("Name")?.Value));
                CreateGroup(plcProjectItem, child);
            }

            //Create main with the given instances
            CreateMain(mainPrg, instances);
        });
    }
    
    private static void CreateMain(ITcSmTreeItem? main, List<PouInstanceCall> instances)
    {
        Retry.Invoke(() =>
        {
            var fbSystem = new PouInstanceCall(name: "fbSystem", type: "FB_System");
            
            instances.Insert(0, fbSystem);
            PouStandardization(main, instances);
            instances.RemoveAt(0);

            //Create Cycle
            var cycle = CreatePouAction(main, "Cycle");

            //fbSystem and HiL calls on top...
            var implementationText = XmlFile.HilPrograms?.Aggregate(
                fbSystem.ImplementationText, 
                (current, element) => $"{current}\tPRG_{element}();\n");
            
            //...followed by instance calls
            implementationText = instances.Aggregate(
                implementationText, 
                (current, element) => $"{current}{element.ImplementationText}");
            
            Implementation(cycle, implementationText, true);
        });
    }
    
    private static void CreateGroup(ITcSmTreeItem? parent, XElement? group, string? parentName = "")
    {
        Retry.Invoke(() =>
        {
            var instances = new List<PouInstanceCall>();
            
            var name = group?.Attribute("Name")?.Value;

            if (group is null) return;
            
            name = name?.TcPlcCompatibleString();
            
            var fbName = parentName == "" ? name : $"{parentName}_{name}";
            fbName = fbName?.TcPlcCompatibleString();

            var folder = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);

            foreach (var child in group.Elements())
            {
                if (child.Name == "Device")
                {
                    var attr = child.Attribute("Comment");
                    var comm = attr is null ? "" : attr.Value;
                    instances.Add(new PouInstanceCall(name: 
                        child.Attribute("Name")?.Value.TcPlcCompatibleString(), 
                        type: child.Attribute("Type")?.Value, comment: comm));
                }

                if (child.Name != "Group") continue;
                var type = $"{fbName}_{child.Attribute("Name")?.Value}";
                instances.Add(new PouInstanceCall(
                    name: child.Attribute("Name")?.Value.TcPlcCompatibleString(), 
                    type: type));
                CreateGroup(folder, child, fbName);
            }

            CreateFb(folder, fbName, instances);
        });
    }
    
    private static void CreateFb(ITcSmTreeItem? parent, string? name, IReadOnlyCollection<PouInstanceCall> instances)
    {
        Retry.Invoke(() =>
        {
            var fb = parent?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCPOUFB);
            
            PouStandardization(fb, instances);

            //Create cycle
            var cycle = CreatePouAction(fb, "Cycle");

            //Cycle implementation
            Implementation(cycle, instances.Select(x => x.ImplementationText), true);
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
    
    private static void PouStandardization(ITcSmTreeItem? pou, IReadOnlyCollection<PouInstanceCall> instances)
    {
        if (pou is null) return;
        
        Retry.Invoke(() =>
        {
            //Get and clean up implementation and declaration
            var decl = (ITcPlcDeclaration)pou;
            var impl = (ITcPlcImplementation)pou;
            impl.ImplementationText = Cleanup(impl.ImplementationText);
            decl.DeclarationText = Cleanup(decl.DeclarationText);

            //Find and clean up or create actions
            var initRun = CreatePouAction(pou, "InitRun");

            //InitRun
            Implementation(initRun, "\tIF NOT bInitRun THEN RETURN; END_IF\n\tbInitRun := FALSE;\n", true);

            //Implementation
            Implementation(impl, "\tInitRun();\n\tCycle();\n", true);

            //Declaration
            if (instances.Count > 0)
            {
                Declaration(decl, instances.Select(x => x.DeclarationText));
            }
        });
    }
    
    private static void Implementation(ITcPlcImplementation? impl, IEnumerable<string> lines, bool isOnTop = false)
    {
        var text = lines.Aggregate("", (current, line) => current + line);
        Implementation(impl, text, isOnTop);
    }
    
    private static void Implementation(ITcPlcImplementation? impl, string? text, bool isOnTop = false)
    {
        if (impl is null) return;
        
        Retry.Invoke(() =>
        {
            if (text == "") return;

            var implementationText = "";

            if (isOnTop)
            {
                var tmpImpl = impl.ImplementationText;
                implementationText = START_REGION + "\n";
                implementationText += text;
                implementationText += END_REGION + "\n";
                implementationText += tmpImpl;
                impl.ImplementationText = implementationText;
                return;
            }

            implementationText += "\n" + START_REGION + "\n";
            implementationText += text;
            implementationText += END_REGION;
            impl.ImplementationText = implementationText;
        });
    }
    
    private static void Declaration(ITcPlcDeclaration decl, IEnumerable<string> lines)
    {
        var text = lines.Aggregate("", (current, line) => current + line);
        Declaration(decl, text);
    }
    
    private static void Declaration(ITcPlcDeclaration decl, string text)
    {
        Retry.Invoke(() =>
        {
            //Get lines
            var lines = decl.DeclarationText.Split(["\n"], StringSplitOptions.RemoveEmptyEntries);
            
            //Get all text
            var tempDecl = decl.DeclarationText;
            
            //Remove first line from text (which is the program/fb definition e.g. PROGRAM MAIN)
            if (lines.Any()) tempDecl = tempDecl.Replace(lines[0], "");
            
            //Add first line to declaration
            var declarationText = lines.Any() ? lines[0] : "";
            
            //Add generated text to declaration
            declarationText += "\n" + START_REGION + "\n";
            declarationText += "VAR_INPUT\n";
            declarationText += "\tbInitRun : BOOL := TRUE;\n";
            declarationText += text;
            declarationText += "END_VAR\n";
            declarationText += END_REGION;
             
            //Add rest of existing text to declaration
            declarationText += tempDecl;

            decl.DeclarationText = declarationText;
        });
    }
    
    private static string Cleanup(string text)
    {
        var lines = text.Split('\n');
        var ret = "";

        var startTag = new Regex("^\t?" + START_REGION);
        var endTag = new Regex("^\t?" + END_REGION);
        var isGeneratedText = false;

        foreach (var line in lines)
        {
            if (!isGeneratedText)
            {
                if (startTag.IsMatch(line))
                {
                    isGeneratedText = true;
                    continue;
                }
            }

            if (isGeneratedText)
            {
                if (endTag.IsMatch(line))
                {
                    isGeneratedText = false;
                    continue;
                }
            }

            if (!isGeneratedText)
                ret += $"{line}\n";
        }

        ret = ret
            .Replace("VAR_INPUT\nEND_VAR\n", "")
            .Replace("VAR_OUTPUT\nEND_VAR\n", "")
            .Replace("VAR\nEND_VAR\n", "");

        return string.IsNullOrEmpty(ret) ? ret : ret.Remove(ret.Length - 1);
    }
}