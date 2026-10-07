using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EnvDTE;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat.Automation;

/// <summary>
/// Generator for Task variables.
/// </summary>
internal static class TaskGenerator
{
    private const string SimulationInterfaceAttribute = "simulation_interface";

    public static string TaskName { get; private set; } = "Main";
    public static string Filter { get; private set; } = "^MAIN\\.";

    public static bool SetFilter(string taskName, string filter)
    {
        if (!taskName.IsBasicCharacters())
        {
            Logger.LogWarning(typeof(TaskGenerator), "TaskName has invalid characters");
            return false;
        }

        try
        {
            _ = new Regex(filter, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException e)
        {
            Logger.LogWarning(typeof(TaskGenerator), $"Invalid variable filter '{filter}': {e.Message}");
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
        
        var instance = tcSysManager?.GetPlcInstance();
        if (instance is null)
        {
            Logger.LogWarning(typeof(TaskGenerator), "No Plc instance found");
            return;
        }
        
        var (filter, symbolSource) = tcSysManager.GetSymbolsWithAttribute(instance, SimulationInterfaceAttribute);
        if (filter.Count == 0)
        {
            Logger.LogWarning(typeof(TaskGenerator),
                $"No symbols with '{SimulationInterfaceAttribute}' found. " +
                "Build the Plc project and run Create Task again.");
            return;
        }

        Logger.LogInfo(typeof(TaskGenerator),
            $"Create Task '{TaskName}', filter '{Filter}'", verbose: true);
        Logger.LogInfo(typeof(TaskGenerator),
            $"Found {filter.Count} '{SimulationInterfaceAttribute}' symbols ({symbolSource})", verbose: true);

        var nameFilter = new Regex(Filter, RegexOptions.IgnoreCase);
        var harvestFilterMatchCount = filter.Count(name => nameFilter.IsMatch(name));
        Logger.LogInfo(typeof(TaskGenerator),
            harvestFilterMatchCount == 0
                ? $"Harvested {filter.Count} {SimulationInterfaceAttribute} symbols; none match the filter."
                : $"Harvested {filter.Count} {SimulationInterfaceAttribute} symbols; {harvestFilterMatchCount} match the filter.");

        var task = tcSysManager?
            .GetItem($"{TcShortcut.NODE_RT_TASKS}")?
            .GetOrCreateChild(TaskName, (int)TcSmTreeItemSubType.TaskWithImage);
        
        if (task is null)
        {
            Logger.LogWarning(typeof(TaskGenerator), "Error creating task");
            return;
        }
           
        if (task.ItemSubType == (int)TcSmTreeItemSubType.TaskWithoutImage)
        {
            Logger.LogWarning(typeof(TaskGenerator), "Task has no image");
            return;
        }
        
        var inputVariables = new List<ITcSmTreeItem>();
        var outputVariables = new List<ITcSmTreeItem>();

        var instanceVarGroups = instance.GetVarGroups();

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

        Logger.LogInfo(typeof(TaskGenerator),
            $"Name filter matched {inputVariables.Count} inputs, {outputVariables.Count} outputs", verbose: true);

        LogCreateTaskDiagnostics(
            symbolSource,
            filter,
            harvestFilterMatchCount,
            inputVariables.Count,
            outputVariables.Count,
            instance,
            nameFilter);

        if (inputVariables.Count == 0 && outputVariables.Count == 0)
        {
            Logger.LogWarning(typeof(TaskGenerator),
                $"Create Task linked 0 variables (harvest={filter.Count}, filterMatch={harvestFilterMatchCount}, " +
                $"instanceIn=0, instanceOut=0). Empty task shell may still exist. Filter='{Filter}'. " +
                "See CreateTaskDiag above / TaskGenDiag file in AppData.");
            return;
        }
        
        var taskVarGroups = task.GetVarGroups().ToList();
        var hasTaskInputs = taskVarGroups.Any(group => group.ItemSubType == 1);
        var hasTaskOutputs = taskVarGroups.Any(group => group.ItemSubType == 2);
        if (!hasTaskInputs || !hasTaskOutputs)
        {
            var missing = new List<string>();
            if (!hasTaskInputs) missing.Add("Inputs");
            if (!hasTaskOutputs) missing.Add("Outputs");
            Logger.LogWarning(typeof(TaskGenerator),
                $"Task '{task.Name}' has no {string.Join(" or ", missing)} variable group(s). " +
                "The task node exists, but variables cannot be created until Inputs/Outputs are present.");
            return;
        }

        var createdInputs = 0;
        var skippedInputs = 0;
        var createdOutputs = 0;
        var skippedOutputs = 0;
        
        foreach (var varGroup in taskVarGroups)
        {
            switch (varGroup.ItemSubType)
            {
                case 1:
                    (createdInputs, skippedInputs) = varGroup.AddAndLinkVariables(outputVariables);
                    Logger.LogInfo(typeof(TaskGenerator),
                        $"Task Inputs: created {createdInputs} / skipped {skippedInputs}", verbose: true);
                    break;
                case 2:
                    (createdOutputs, skippedOutputs) = varGroup.AddAndLinkVariables(inputVariables);
                    Logger.LogInfo(typeof(TaskGenerator),
                        $"Task Outputs: created {createdOutputs} / skipped {skippedOutputs}", verbose: true);
                    break;
            }
        }

        Logger.LogInfo(typeof(TaskGenerator),
            $"Task variables have been updated ({createdInputs} inputs, {createdOutputs} outputs).");
    }

    extension(IEnumerable item)
    {
        private IEnumerable<ITcSmTreeItem> GetVarGroups()
        {
            return item
                .Cast<ITcSmTreeItem>()
                .Where(varGroup => varGroup.ItemType == (int)TREEITEMTYPES.TREEITEMTYPE_VARGRP).ToList();
        }

        private void CollectVariablesRecursive(ICollection<ITcSmTreeItem> variables, HashSet<string> filter, Regex nameFilter)
        {
            var childItems = item.Cast<ITcSmTreeItem>();
        
            foreach (var childItem in childItems)
            {
                if (childItem.Name.EndsWith('.'))
                {
                    childItem.CollectVariablesRecursive(variables, filter, nameFilter);
                    continue;
                }

                if (TryGetHarvestMatchName(childItem, filter, nameFilter, out _))
                {
                    variables.Add(childItem);
                }
            }
        }
    }

    extension(ITcSysManager15? sysManager)
    {
        private (HashSet<string> Symbols, string Source) GetSymbolsWithAttribute(ITcSmTreeItem instance, string attribute)
        {
            if (instance.CastTo<ITcModuleInstance2>() is { } moduleInstance)
            {
                var fromExport = ParseSymbolsWithAttribute(moduleInstance.ExportXml(), attribute);
                if (fromExport.Count > 0) return (fromExport, "instance ExportXml");
            }

            var fromProduce = ParseSymbolsWithAttribute(instance.ProduceXml(), attribute);
            if (fromProduce.Count > 0) return (fromProduce, "instance ProduceXml");

            var tmcPath = sysManager.FindTmcPath();
            if (tmcPath is null)
            {
                return ([], "none");
            }

            return (ParseSymbolsWithAttribute(File.ReadAllText(tmcPath), attribute), tmcPath);
        }

        private string? FindTmcPath()
        {
            var plcName = XmlFile.Instance.PlcProjectName;
            var roots = new List<string>();
            var relativeFiles = new List<string>();

            if (!string.IsNullOrEmpty(XmlFile.Instance.Path) &&
                Path.GetDirectoryName(XmlFile.Instance.Path) is { } assistantFolder)
            {
                roots.Add(assistantFolder);
            }

            try
            {
                if (sysManager is not null &&
                    ((DTE)sysManager.DTE).GetProjectFolder() is { } projectFolder)
                {
                    roots.Add(projectFolder);
                }
            }
            catch
            {
                // DTE path is optional when XmlFile already points at the TwinCAT project folder.
            }

            try
            {
                var plcNode = sysManager?.GetItem($"{TcShortcut.NODE_PLC_CONFIG}^{plcName}");
                if (plcNode is not null)
                {
                    foreach (var value in XElement.Parse(plcNode.ProduceXml()).Descendants().Select(x => x.Value))
                    {
                        if (value.Contains(".plcproj", StringComparison.OrdinalIgnoreCase) ||
                            value.Contains(".tmc", StringComparison.OrdinalIgnoreCase))
                        {
                            relativeFiles.Add(value.Trim());
                        }
                    }
                }
            }
            catch
            {
                // Fall back to conventional TwinCAT folder layout.
            }

            foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                foreach (var relative in relativeFiles)
                {
                    var fullPath = Path.IsPathRooted(relative)
                        ? relative
                        : Path.GetFullPath(Path.Combine(root, relative));

                    var tmcPath = fullPath.EndsWith(".tmc", StringComparison.OrdinalIgnoreCase)
                        ? fullPath
                        : Path.ChangeExtension(fullPath, ".tmc");

                    if (File.Exists(tmcPath)) return tmcPath;
                }

                var conventional = Path.Combine(root, plcName ?? string.Empty, $"{plcName}.tmc");
                if (File.Exists(conventional)) return conventional;
            }

            foreach (var root in roots.Distinct(StringComparer.OrdinalIgnoreCase).Where(Directory.Exists))
            {
                var match = Directory
                    .EnumerateFiles(root, $"{plcName}.tmc", SearchOption.AllDirectories)
                    .FirstOrDefault(path =>
                        !path.Contains($"{Path.DirectorySeparatorChar}_Libraries{Path.DirectorySeparatorChar}",
                            StringComparison.OrdinalIgnoreCase));
                if (match is not null) return match;
            }

            return null;
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

        private (int Created, int Skipped) AddAndLinkVariables(List<ITcSmTreeItem> variables)
        {
            varGroup.DeleteAllVariables();
            var created = 0;
            var skipped = 0;

            foreach (var variable in variables)
            {
                var xElement = XElement.Parse(variable.ProduceXml());
                var type = xElement.Descendants("VarType").FirstOrDefault();
                if (type is null)
                {
                    skipped++;
                    Logger.LogWarning(typeof(TaskGenerator), $"No VarType for '{variable.Name}'");
                    continue;
                }

                ITcSmTreeItem? child;
                try
                {
                    child = varGroup.CreateChild(variable.Name, -1, null, type.Value);
                }
                catch (Exception e)
                {
                    skipped++;
                    Logger.LogWarning(typeof(TaskGenerator),
                        $"Failed to create '{variable.Name}' ({type.Value}): {e.Message}");
                    continue;
                }

                if (child.CastTo<ITcVariable2>() is not { } var)
                {
                    skipped++;
                    Logger.LogWarning(typeof(TaskGenerator),
                        $"Created '{variable.Name}' but could not access it as a task variable");
                    continue;
                }

                try
                {
                    var.AddLinkToVariable(variable.PathName);
                }
                catch (Exception e)
                {
                    Logger.LogWarning(typeof(TaskGenerator),
                        $"Failed to link '{variable.Name}': {e.Message}");
                }

                created++;
            }

            return (created, skipped);
        }
    }

    private static HashSet<string> ParseSymbolsWithAttribute(string? xml, string attribute)
    {
        if (string.IsNullOrWhiteSpace(xml)) return [];

        try
        {
            return XDocument.Parse(xml)
                .Descendants("Symbol")
                .Where(symbol =>
                    symbol.Element("Properties")?
                        .Elements("Property")
                        .Any(property => property.Element("Name")?.Value == attribute) == true)
                .Select(symbol => symbol.Element("Name")?.Value)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return [];
        }
    }

    private static bool TryGetHarvestMatchName(
        ITcSmTreeItem item,
        HashSet<string> harvested,
        Regex nameFilter,
        out string matchName)
    {
        foreach (var candidate in GetSymbolMatchCandidates(item))
        {
            if (harvested.Contains(candidate) && nameFilter.IsMatch(candidate))
            {
                matchName = candidate;
                return true;
            }
        }

        matchName = string.Empty;
        return false;
    }

    /// <summary>
    /// Possible PLC symbol names for a tree leaf (same shape as TMC / ExportXml harvest entries).
    /// </summary>
    private static IEnumerable<string> GetSymbolMatchCandidates(ITcSmTreeItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Name) && !item.Name.EndsWith('.'))
        {
            yield return item.Name;
        }

        var fromPathName = GetSymbolNameFromPathName(item.PathName);
        if (fromPathName is not null &&
            !fromPathName.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
        {
            yield return fromPathName;
        }
    }

    /// <summary>
    /// TwinCAT PathName uses '^'; the linked PLC symbol is usually the last segment.
    /// </summary>
    private static string? GetSymbolNameFromPathName(string? pathName)
    {
        if (string.IsNullOrWhiteSpace(pathName) || pathName.EndsWith('.'))
        {
            return null;
        }

        var separator = pathName.LastIndexOf('^');
        if (separator < 0 || separator >= pathName.Length - 1)
        {
            return null;
        }

        return pathName[(separator + 1)..];
    }

    private static void LogCreateTaskDiagnostics(
        string symbolSource,
        HashSet<string> harvested,
        int filterMatchCount,
        int instanceInputs,
        int instanceOutputs,
        ITcSmTreeItem instance,
        Regex nameFilter)
    {
        Logger.LogInfo(typeof(TaskGenerator),
            $"CreateTaskDiag: source={symbolSource} harvested={harvested.Count} " +
            $"filterMatch={filterMatchCount} instanceIn={instanceInputs} instanceOut={instanceOutputs}");
        Logger.LogInfo(typeof(TaskGenerator), $"CreateTaskDiag: filter='{Filter}'");

        var zeroMatch = instanceInputs == 0 && instanceOutputs == 0;
        var sampleLines = new List<string>();

        if (zeroMatch)
        {
            var leaves = new List<(ITcSmTreeItem Item, string Name, string? PathName)>();
            foreach (var varGroup in instance.GetVarGroups())
            {
                CollectLeafDiagnostics(varGroup, leaves, 200);
            }

            if (leaves.Count == 0)
            {
                var emptyTree = "sampleFail: no PLC variable leaves under instance Inputs/Outputs " +
                                "(tree may be empty or not loaded).";
                sampleLines.Add(emptyTree);
                Logger.LogInfo(typeof(TaskGenerator), emptyTree);
            }
            else
            {
                foreach (var leaf in leaves.Take(5))
                {
                    var resolved = GetSymbolMatchCandidates(leaf.Item)
                        .FirstOrDefault(c => harvested.Contains(c) && nameFilter.IsMatch(c));
                    resolved ??= GetSymbolMatchCandidates(leaf.Item).FirstOrDefault() ?? leaf.Name;
                    var line =
                        $"sampleFail: Name='{leaf.Name}' resolved='{resolved}' " +
                        $"inHarvest={harvested.Contains(resolved)} regex={nameFilter.IsMatch(resolved)}";
                    sampleLines.Add(line);
                    Logger.LogInfo(typeof(TaskGenerator), line);
                }
            }
        }

        var matchingHarvest = harvested
            .Where(name => nameFilter.IsMatch(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var safeTask = SanitizeFileNamePart(TaskName);
            var filePath = Path.Combine(AppData.Path, $"TaskGenDiag_{safeTask}_{timestamp}.txt");

            var sb = new StringBuilder();
            sb.AppendLine("OC Assistant — Create Task diagnostics");
            sb.AppendLine($"TaskName={TaskName}");
            sb.AppendLine($"Filter={Filter}");
            sb.AppendLine($"Source={symbolSource}");
            sb.AppendLine($"Harvested={harvested.Count}");
            sb.AppendLine($"FilterMatch={filterMatchCount}");
            sb.AppendLine($"InstanceInputs={instanceInputs}");
            sb.AppendLine($"InstanceOutputs={instanceOutputs}");
            sb.AppendLine();
            sb.AppendLine("Interpretation:");
            if (filterMatchCount == 0)
                sb.AppendLine("- filterMatch=0: regex does not match harvested symbol names (check Filter).");
            else if (zeroMatch)
                sb.AppendLine("- filterMatch>0 but instanceIn/Out=0: harvest+regex OK; instance tree walk found no matching leaves.");
            else
                sb.AppendLine("- instanceIn/Out>0: matching succeeded; linking should follow.");
            if (zeroMatch)
                sb.AppendLine("- Empty task shell may still exist; 0 variables were linked.");
            sb.AppendLine();
            sb.AppendLine("Harvest names matching filter (first 5):");
            if (matchingHarvest.Count == 0)
                sb.AppendLine("  (none)");
            else
                foreach (var name in matchingHarvest)
                    sb.AppendLine($"  {name}");

            if (sampleLines.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Instance leaf samples (when 0 linked):");
                foreach (var line in sampleLines)
                    sb.AppendLine($"  {line}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            Logger.LogInfo(typeof(TaskGenerator), $"CreateTaskDiag file: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(typeof(TaskGenerator), $"Could not write CreateTaskDiag file: {ex.Message}");
        }
    }

    private static string SanitizeFileNamePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Task";

        var sanitized = value;
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(invalid, '_');
        }

        return sanitized;
    }

    private static void CollectLeafDiagnostics(
        ITcSmTreeItem item,
        List<(ITcSmTreeItem Item, string Name, string? PathName)> leaves,
        int maxLeaves)
    {
        if (leaves.Count >= maxLeaves) return;

        foreach (ITcSmTreeItem child in item)
        {
            if (leaves.Count >= maxLeaves) return;

            if (child.Name.EndsWith('.'))
            {
                CollectLeafDiagnostics(child, leaves, maxLeaves);
                continue;
            }

            leaves.Add((child, child.Name, child.PathName));
        }
    }
}
