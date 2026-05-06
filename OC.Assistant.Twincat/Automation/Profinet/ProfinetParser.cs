using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat.Automation.Profinet;

internal class ProfinetParser
{
    public List<ProfinetVariable> Variables { get; }
    public List<SafetyModule> SafetyModules { get; }

    public ProfinetParser(string xtiPath, string pnName)
    {
        var subModules = XDocument
            .Load(xtiPath)
            .Descendants("Box")
            .Where(elem => elem.Attribute("Disabled") is null)
            .Descendants("Name")
            .Where(elem => elem.Parent?.Name == "SubModule" && !elem.Value.Contains("#ignore"))
            .Select(elem => elem.Parent)
            .ToList();
        
        var inputs = subModules
            .Descendants("BitOffs")
            .Where(elem =>
                elem.Parent?.Parent?.Attribute("VarGrpType")?.Value == "1" &&
                elem.Parent?.Parent?.Parent?.Name != "Box")
            .Select(elem => new ProfinetVariable(elem.Parent, pnName, true));

        var outputs = subModules
            .Descendants("BitOffs")
            .Where(elem =>
                elem.Parent?.Parent?.Attribute("VarGrpType")?.Value == "2" &&
                elem.Parent?.Parent?.Parent?.Name != "Box")
            .Select(elem => new ProfinetVariable(elem.Parent, pnName, false));
        
        var variables = new Dictionary<string, ProfinetVariable>();
        
        foreach (var variable in inputs.Concat(outputs))
        {
            if (!variables.TryAdd(variable.Name, variable))
            {
                Logger.LogWarning(this, $"{variable.Name} exists more than once");
            }
        }
        
        SafetyModules = subModules
            .Descendants("Name")
            .Where(elem => elem.Value.Contains("#failsafe"))
            .Select(elem => new SafetyModule(elem.Parent, pnName))
            .ToList();

        foreach (var module in SafetyModules)
        {
            if (variables.TryGetValue(module.HstDataName, out var hstVar))
            {
                hstVar.SafetyFlag = true;
                module.HstVariable = hstVar;
            }
            if (variables.TryGetValue(module.DevDataName, out var devVar))
            {
                devVar.SafetyFlag = true;
                module.DevVariable = devVar;
            }
        }

        Variables = variables.Values
            .OrderBy(x => GetCategoryOrder(x.Name))
            .ThenBy(x => GetAddressNumber(x.Name))
            .ToList();
    }
    
    private static int GetCategoryOrder(string name)
    {
        if (name.StartsWith('I'))
        {
            return 0;
        }
        return name.StartsWith('Q') ? 1 : 2;
    }

    private static int GetAddressNumber(string name)
    {
        if (name.Length <= 1 || (!name.StartsWith('I') && !name.StartsWith('Q')))
        {
            return int.MaxValue;
        }
        var numPart = name[1..];
        return int.TryParse(numPart, out var number) ? number : int.MaxValue;
    }
}