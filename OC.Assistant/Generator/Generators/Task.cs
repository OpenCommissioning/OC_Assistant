using System.Collections;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
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
    /// <param name="tcSysManager">The connected <see cref="ITcSysManager15"/> interface.</param>
    public static bool CreateVariables(ITcSysManager15? tcSysManager)
    {
        return Retry.Invoke(bool () =>
        {
            //Get plc instance
            var instance = tcSysManager?.TryGetPlcInstance();
            if (instance is null) return false;
            
            //Collect all symbols with 'simulation_interface' attribute
            var filter = instance.GetSymbolsWithAttribute("simulation_interface");
            
            //Get task
            var task = tcSysManager?.TryGetItem(TcShortcut.TASK, XmlFile.XmlBase.PlcTaskName);
            if (task is null)
            {
                Logger.LogWarning(typeof(Task), "Task not found");
                return false;
            }
               
            //Task has no image
            if (task.ItemSubType == (int)TcSmTreeItemSubType.TaskWithoutImage)
            {
                Logger.LogWarning(typeof(Task), "Task has no image");
                return false;
            }
            
            var inputVariables = new List<ITcSmTreeItem>();
            var outputVariables = new List<ITcSmTreeItem>();

            var instanceVarGroups = instance.GetVarGroups();
            if (instanceVarGroups is null)
            {
                return false;
            }
                
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
            if (taskVarGroups is null)
            {
                return false;
            }
            
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

            return true;
        });
    }

    private static HashSet<string?> GetSymbolsWithAttribute(this ITcSmTreeItem instance, string attribute)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        return XDocument.Parse(((ITcModuleInstance2) instance).ExportXml())
            .Descendants("Symbol")
            .Where(symbol => 
                symbol.Element("Properties")?
                    .Element("Property")?
                    .Element("Name")?.Value == attribute)
            .Select(symbol => symbol.Element("Name")?.Value)
            .Distinct()
            .ToHashSet();
    }

    private static IEnumerable<ITcSmTreeItem>? GetVarGroups(this IEnumerable item)
    {
        return Retry.Invoke(() =>
        {
            return item
                .Cast<ITcSmTreeItem>()
                .Where(varGroup => varGroup.ItemType == (int)TREEITEMTYPES.TREEITEMTYPE_VARGRP);
        });
    }
    
    private static void CollectVariablesRecursive(this IEnumerable item, ICollection<ITcSmTreeItem> variables, HashSet<string?> filter, bool isTopLevel = true)
    {
        Retry.Invoke(() =>
        {
            var filteredChildItems = item
                .Cast<ITcSmTreeItem9>()
                .Where(childItem => !isTopLevel || childItem.Name.StartsWith("MAIN.", StringComparison.CurrentCultureIgnoreCase));
        
            foreach (var childItem in filteredChildItems)
            {
                if (childItem.Name.EndsWith('.'))
                {
                    childItem.CollectVariablesRecursive(variables, filter, false);
                    continue;
                }

                if (filter.Contains(childItem.Name))
                {
                    variables.Add(childItem);
                }
            }
        });
    }

    private static void DeleteAllVariables(this ITcSmTreeItem varGroup)
    {
        Retry.Invoke(() =>
        {
            foreach (ITcSmTreeItem variable in varGroup)
            {
                varGroup.DeleteChild(variable.Name);
            }
        });
    }

    private static void AddAndLinkVariables(this ITcSmTreeItem varGroup, List<ITcSmTreeItem> variables)
    {
        Retry.Invoke(() =>
        {
            varGroup.DeleteAllVariables();
            foreach (var variable in variables)
            {
                var xElement = XElement.Parse(variable.ProduceXml());
                var type = xElement.Descendants("VarType").FirstOrDefault();
                if (type == default) continue;
                // ReSharper disable once SuspiciousTypeConversion.Global
                var var = (ITcVariable2) varGroup.CreateChild(variable.Name, -1, null, type.Value);
                var.AddLinkToVariable(variable.PathName);
            }
        });
    }
}