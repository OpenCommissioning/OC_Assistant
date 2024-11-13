using System.Xml.Linq;

namespace OC.Assistant.Generator.Profinet;

internal class ProfinetParser
{
    public List<ProfinetVariable> Variables { get; }
    public List<SafetyModule> SafetyModules { get; }

    public ProfinetParser(string xtiPath, string pnName)
    {
        var xtiDoc = XDocument.Load(xtiPath);

        var activeBoxes = (from elem in xtiDoc.Descendants("Box")
            where elem.Attribute("Disabled") is null
            select elem).ToList();
            
        var subModules = (from elem in activeBoxes.Descendants("Name")
            where elem.Parent?.Name == "SubModule" 
                  && !elem.Value.Contains(ProfinetTags.BUS_IGNORE)
            select elem.Parent).ToList();
            
        var inputs = from elem in subModules.Descendants("BitOffs")
            where elem.Parent?.Parent?.Attribute("VarGrpType")?.Value == "1" &&
                  elem.Parent.Parent.Parent?.Name != "Box"
            select new ProfinetVariable(elem.Parent, pnName, true);
            
        var outputs = from elem in subModules.Descendants("BitOffs")
            where elem.Parent?.Parent?.Attribute("VarGrpType")?.Value == "2" &&
                  elem.Parent.Parent.Parent?.Name != "Box"
            select new ProfinetVariable(elem.Parent, pnName, false);
            
        Variables = inputs.Concat(outputs).ToList();
            
        SafetyModules = (from elem in subModules.Descendants("Name")
            where elem.Value.Contains(ProfinetTags.BUS_LP) || elem.Value.Contains(ProfinetTags.BUS_XP)
            select new SafetyModule(elem.Parent, pnName)).ToList();
    }
}