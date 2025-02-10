using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator;

internal class PouInstance
{
    private readonly string? _name;
    private readonly string? _type;
    private readonly string? _label;
    private readonly string? _controlAddress;
    private readonly string? _statusAddress;
    private readonly string? _assignments;
    

    public PouInstance(string? name, string? type)
    {
        _name = name?.TcPlcCompatibleString();
        _type = type;
    }
    
    public PouInstance(XElement element)
    {
        _name = element.Attribute("Name")?.Value.TcPlcCompatibleString();
        _type = element.Attribute("Type")?.Value;
        
        _label = element.Element("Label")?.Value;
        
        //var address = element.Element("Address");
        _controlAddress = null;// address?.Attribute("Control")?.Value;
        _statusAddress = null;// address?.Attribute("Status")?.Value;

        _assignments = element
            .Elements()
            .Where(x => x.Name.LocalName is "Control" or "In" or "Address")
            .Aggregate("", (current, next) => 
                current + $"\n\t\t{next.Attribute("Name")?.Value} := GVL_{next.Attribute("Assignment")?.Value},");

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
    

    public string InitRunText
    {
        get
        {
            if (string.IsNullOrEmpty(_controlAddress) || string.IsNullOrEmpty(_statusAddress))
            {
                return "";
            }
            return $"\t{_name}.AssignProcessData(aControlData := GVL_{_controlAddress}, aStatusData := GVL_{_statusAddress});\n";
        }
    }
}