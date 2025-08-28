using System.Xml.Linq;

namespace OC.Assistant.Twincat;

internal class PouInstance
{
    private readonly string? _name;
    private readonly string? _type;
    private readonly string? _label;
    private readonly string? _assignments;

    public PouInstance(string? name, string? type)
    {
        _name = name?.MakePlcCompatible();
        _type = type?.MakePlcCompatible();
    }
    
    public PouInstance(XElement element)
    {
        _name = element.Attribute("Name")?.Value.MakePlcCompatible();
        _type = element.Attribute("Type")?.Value.MakePlcCompatible();
        
        _label = element.Element("Label")?.Value;
        
        _assignments = element
            .Elements()
            .Where(x => x.Name.LocalName is "Control" or "In")
            .Aggregate("", (current, next) => 
                current + $"\n\t\t{next.Attribute("Name")?.Value} := GVL_{next.Attribute("Assignment")?.Value},");
        
        _assignments = element
            .Elements()
            .Where(x => x.Name.LocalName is "Address")
            .Aggregate(_assignments, (current, next) => 
                current + $"\n\t\t{next.Attribute("Name")?.Value} := ADR(GVL_{next.Attribute("Assignment")?.Value}),");

        _assignments = element
            .Elements()
            .Where(x => x.Name.LocalName is "Status" or "Out")
            .Aggregate(_assignments, (current, next) => 
                current + $"\n\t\t{next.Attribute("Name")?.Value} => GVL_{next.Attribute("Assignment")?.Value},");

        if (_assignments.Length > 0)
        {
            _assignments = _assignments.Remove(_assignments.Length - 1);
        }
    }

    public string DeclarationText
    {
        get
        {
            var declaration = $"\t{_name} : {_type};";
            if (!string.IsNullOrEmpty(_label))
            {
                declaration += $" //{_label}";
            }
            return declaration + "\n";
        }
    }
    
    public string ImplementationText => $"\t{_name}({_assignments});\n";
    
    public string InitRunText => "";
}