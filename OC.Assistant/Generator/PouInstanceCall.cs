using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator;

internal class PouInstanceCall
{
    private readonly string? _name;
    private readonly string? _type;
    private readonly string? _comment;
    private readonly string? _controlAddress;
    private readonly string? _statusAddress;
    private readonly string? _assignments;
    

    public PouInstanceCall(string? name, string? type)
    {
        _name = name?.TcPlcCompatibleString();
        _type = type;
    }
    
    public PouInstanceCall(XElement element)
    {
        _name = element.Attribute("Name")?.Value.TcPlcCompatibleString();
        _type = element.Attribute("Type")?.Value;
        
        _comment = element.Element("Comment")?.Value;
        
        var address = element.Element("Address");
        _controlAddress = address?.Attribute("Control")?.Value;
        _statusAddress = address?.Attribute("Status")?.Value;

        _assignments = element
            .Elements("Control")
            .Aggregate("", (current, next) => 
                current + $"\r\n\t{next.Attribute("Name")?.Value} := GVL_{next.Attribute("Assignment")?.Value},");

        _assignments = element
            .Elements("Status")
            .Aggregate(_assignments, (current, next) => 
                current + $"\r\n\t{next.Attribute("Name")?.Value} => GVL_{next.Attribute("Assignment")?.Value},");

        if (_assignments.Length > 0)
        {
            _assignments = _assignments.Remove(_assignments.Length - 1) + "\r\n";
        }
    }

    public string DeclarationText
    {
        get
        {
            var declaration = $"\t{_name} : {_type};";
            if (!string.IsNullOrEmpty(_comment))
            {
                declaration += $" // {_comment}";
            }
            return declaration + "\r\n";
        }
    }
    
    public string ImplementationText => $"{_name}({_assignments});\r\n";
    

    public string InitRunText
    {
        get
        {
            if (string.IsNullOrEmpty(_controlAddress) || string.IsNullOrEmpty(_statusAddress))
            {
                return "";
            }
            return $"\t{_name}.AssignProcessData(aControlData := GVL_{_controlAddress}, aStatusData := GVL_{_statusAddress});\r\n";
        }
    }
}