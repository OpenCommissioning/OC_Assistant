using System.Collections;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Generators;

/// <summary>
/// Generator for Task variables.
/// </summary>
internal static class Task
{
    /// <summary>
    /// Creates variables for a task, based on the plc instance.
    /// </summary>
    public static void CreateVariables(ITcSysManager15? tcSysManager)
    {
        tcSysManager?.SaveProject();
        
        //Get plc instance
        var instance = tcSysManager?.TryGetPlcInstance();
        if (instance is null) return;
        
        //Collect all symbols with 'simulation_interface' attribute
        var filter = instance.GetSymbolsWithAttribute("simulation_interface");
        
        //Get task
        var task = tcSysManager?.TryGetItem(TcShortcut.TASK, XmlFile.Instance.PlcTaskName);
        if (task is null)
        {
            Logger.LogWarning(typeof(Task), "Task not found");
            return;
        }
           
        //Task has no image
        if (task.ItemSubType == (int)TcSmTreeItemSubType.TaskWithoutImage)
        {
            Logger.LogWarning(typeof(Task), "Task has no image");
            return;
        }
        
        var inputVariables = new List<ITcSmTreeItem>();
        var outputVariables = new List<ITcSmTreeItem>();

        var instanceVarGroups = instance.GetVarGroups();
            
        //Collect variables from plc instance
        foreach (var varGroup in instanceVarGroups)
        {
            switch (varGroup.ItemSubType)
            {
                case 1:
                    varGroup.CollectVariablesRecursive(inputVariables, filter);
                    break;
                case 2:
                    varGroup.CollectVariablesRecursive(outputVariables, filter);
                    break;
            }
        }
        
        var taskVarGroups = task.GetVarGroups();
        
        //Create and link task variables 
        foreach (var varGroup in taskVarGroups)
        {
            switch (varGroup.ItemSubType)
            {
                case 1:
                    varGroup.AddAndLinkVariables(outputVariables);
                    break;
                case 2:
                    varGroup.AddAndLinkVariables(inputVariables);
                    break;
            }
        }

        Logger.LogInfo(typeof(Task), "Task variables have been updated.");
    }

    private static HashSet<string?> GetSymbolsWithAttribute(this ITcSmTreeItem instance, string attribute)
    {
        if (instance.CastTo<ITcModuleInstance2>() is not {} moduleInstance) return [];
        return XDocument.Parse(moduleInstance.ExportXml())
            .Descendants("Symbol")
            .Where(symbol => 
                symbol.Element("Properties")?
                    .Element("Property")?
                    .Element("Name")?.Value == attribute)
            .Select(symbol => symbol.Element("Name")?.Value)
            .Distinct()
            .ToHashSet();
    }

    private static IEnumerable<ITcSmTreeItem> GetVarGroups(this IEnumerable item)
    {
        return item
            .Cast<ITcSmTreeItem>()
            .Where(varGroup => varGroup.ItemType == (int)TREEITEMTYPES.TREEITEMTYPE_VARGRP).ToList();
    }
    
    private static void CollectVariablesRecursive(this IEnumerable item, ICollection<ITcSmTreeItem> variables, HashSet<string?> filter)
    {
        var childItems = item.Cast<ITcSmTreeItem>();
        
        foreach (var childItem in childItems)
        {
            if (childItem.Name.EndsWith('.'))
            {
                childItem.CollectVariablesRecursive(variables, filter);
                continue;
            }

            if (filter.Contains(childItem.Name))
            {
                variables.Add(childItem);
            }
        }
    }

    private static void DeleteAllVariables(this ITcSmTreeItem varGroup)
    {
        foreach (ITcSmTreeItem variable in varGroup)
        {
            varGroup.DeleteChild(variable.Name);
        }
    }

    private static void AddAndLinkVariables(this ITcSmTreeItem varGroup, List<ITcSmTreeItem> variables)
    {
        varGroup.DeleteAllVariables();
        foreach (var variable in variables)
        {
            var xElement = XElement.Parse(variable.ProduceXml());
            var type = xElement.Descendants("VarType").FirstOrDefault();
            if (type is null) continue;
            if (varGroup.CreateChild(variable.Name, -1, null, type.Value)
                    .CastTo<ITcVariable2>() is not {} var) continue;
            var.AddLinkToVariable(variable.PathName);
        }
    }
}