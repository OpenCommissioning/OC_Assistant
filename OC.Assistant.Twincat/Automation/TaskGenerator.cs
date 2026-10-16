using System.Collections;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat.Automation;

/// <summary>
/// Generator for Task variables.
/// </summary>
internal static class TaskGenerator
{
    public static string TaskName { get; private set; } = "Main";
    public static string Filter { get; private set; } = "^MAIN\\.";

    public static bool SetFilter(string taskName, string filter)
    {
        if (!taskName.IsBasicCharacters())
        {
            Logger.LogWarning(typeof(TaskGenerator), "TaskName has invalid characters");
            return false;
        }
        
        TaskName = taskName;
        Filter = filter;
        return true;
    }
    
    /// <summary>
    /// Creates variables for a task, based on the plc instance.
    /// </summary>
    public static void CreateVariables(ITcSysManager15? tcSysManager)
    {
        tcSysManager?.SaveProject();
        
        //Get plc instance
        var instance = tcSysManager?.GetPlcInstance();
        if (instance is null) return;
        
        //Collect all symbols with 'simulation_interface' attribute
        var filter = instance.GetSymbolsWithAttribute("simulation_interface");
        
        //Get task or create
        var task = tcSysManager?
            .GetItem($"{TcShortcut.NODE_RT_TASKS}")?
            .GetOrCreateChild(TaskName, (int)TcSmTreeItemSubType.TaskWithImage);
        
        if (task is null)
        {
            Logger.LogWarning(typeof(TaskGenerator), "Error creating task");
            return;
        }
           
        //Task has no image
        if (task.ItemSubType == (int)TcSmTreeItemSubType.TaskWithoutImage)
        {
            Logger.LogWarning(typeof(TaskGenerator), "Task has no image");
            return;
        }
        
        var inputVariables = new List<ITcSmTreeItem>();
        var outputVariables = new List<ITcSmTreeItem>();

        var instanceVarGroups = instance.GetVarGroups();
        var nameFilter = new Regex(Filter, RegexOptions.IgnoreCase);
            
        //Collect variables from plc instance
        foreach (var varGroup in instanceVarGroups)
        {
            switch (varGroup.ItemSubType)
            {
                case 1:
                    varGroup.CollectVariablesRecursive(inputVariables, filter, nameFilter);
                    break;
                case 2:
                    varGroup.CollectVariablesRecursive(outputVariables, filter, nameFilter);
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

        Logger.LogInfo(typeof(TaskGenerator), "Task variables have been updated.");
    }

    extension(IEnumerable item)
    {
        private IEnumerable<ITcSmTreeItem> GetVarGroups()
        {
            return item
                .Cast<ITcSmTreeItem>()
                .Where(varGroup => varGroup.ItemType == (int)TREEITEMTYPES.TREEITEMTYPE_VARGRP).ToList();
        }

        private void CollectVariablesRecursive(ICollection<ITcSmTreeItem> variables, HashSet<string?> filter, Regex nameFilter)
        {
            var childItems = item.Cast<ITcSmTreeItem>();
        
            foreach (var childItem in childItems)
            {
                if (childItem.Name.EndsWith('.'))
                {
                    childItem.CollectVariablesRecursive(variables, filter, nameFilter);
                    continue;
                }

                if (filter.Contains(childItem.Name) && nameFilter.IsMatch(childItem.Name))
                {
                    variables.Add(childItem);
                }
            }
        }
    }

    extension(ITcSmTreeItem varGroup)
    {
        private void DeleteAllVariables()
        {
            foreach (ITcSmTreeItem variable in varGroup)
            {
                varGroup.DeleteChild(variable.Name);
            }
        }

        private void AddAndLinkVariables(List<ITcSmTreeItem> variables)
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

        private HashSet<string?> GetSymbolsWithAttribute(string attribute)
        {
            if (varGroup.CastTo<ITcModuleInstance2>() is not {} moduleInstance) return [];
        
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
    }
}