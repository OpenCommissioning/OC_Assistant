using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat.Profinet;

internal partial class ProfinetVariable
{
    /// <summary>
    /// Name of the variable
    /// </summary>
    public string Name { get; }
        
    /// <summary>
    /// Type of the variable
    /// </summary>
    public string Type { get; }
        
    /// <summary>
    /// Link to the bus variable
    /// </summary>
    public string Link { get; }
        
    /// <summary>
    /// The TwinCAT direction 'I' or 'Q'.
    /// </summary>
    public string Direction { get; }

    /// <summary>
    /// The PLC address, if any.
    /// </summary>
    public PlcAddress PlcAddress { get; }
    
    /// <summary>
    /// The byte array size, if any. -1 means no byte array.
    /// </summary>
    public int ByteArraySize { get; }

    /// <summary>
    /// Indicates that this variable is used by a safety module.
    /// </summary>
    public bool SafetyFlag { get; set; }
        
    public ProfinetVariable(XElement? element, string pnName, bool isInput)
    {
        var nameList = GetFullNamePath(element);
            
        Direction = isInput ? "I" : "Q";
        Link = nameList
            .Aggregate($"{TcShortcut.NODE_IO_DEVICES}^{pnName}", (current, next) => $"{current}^{next}");
        Type = element?.Element("Type")?.Value ?? TcType.Byte.Name();
        
        Name = nameList[^1];
        ByteArraySize = GetByteArraySize(Type);
        PlcAddress = new PlcAddress(Name);

        if (PlcAddress.IsValid)
        {
            return;
        }

        Name = nameList.Where(x => x != "API" && x != "Inputs" && x != "Outputs")
            .Aggregate("", (current, next) => $"{current}_{next}")
            .TcPlcCompatibleString();
    }


    /// <summary>
    /// Generates a declaration string for the GVL.
    /// </summary>
    public string CreateGvlDeclaration()
    {
        var template = SafetyFlag ? 
            $"{Tags.VAR_NAME} : {Tags.VAR_TYPE}; //FAILSAFE\n" : 
            $"{{attribute 'TcLinkTo' := '{Tags.LINK}'}}\n{Tags.VAR_NAME} AT %{Tags.DIRECTION}* : {Tags.VAR_TYPE};\n";
        
        if (!PlcAddress.IsValid || ByteArraySize < 0)
        {
            return template
                .Replace(Tags.VAR_TYPE, Type)
                .Replace(Tags.DIRECTION, Direction)
                .Replace(Tags.LINK, Link)
                .Replace(Tags.VAR_NAME, Name);
        }
        
        var declaration = "";
        for (var i = 0; i < ByteArraySize; i++)
        {
            declaration += template
                .Replace(Tags.VAR_TYPE, TcType.Byte.Name())
                .Replace(Tags.DIRECTION, Direction)
                .Replace(Tags.LINK, $"{Link}[{i}]")
                .Replace(Tags.VAR_NAME, $"{PlcAddress.Direction}{PlcAddress.Address + i}");
        }

        return declaration;
    }
    
    /// <summary>
    /// Generates a declaration string for the safety program.
    /// </summary>
    public string CreatePrgDeclaration()
    { 
        return $"\t{{attribute 'TcLinkTo' := '{Tags.LINK}'}}\n\t{Tags.VAR_NAME} AT %{Tags.DIRECTION}* : {Tags.VAR_TYPE};\n"
            .Replace(Tags.VAR_TYPE, Type)
            .Replace(Tags.DIRECTION, Direction)
            .Replace(Tags.LINK, Link)
            .Replace(Tags.VAR_NAME, Name);
    }
    
    private static int GetByteArraySize(string type)
    {
        var regex = ByteArrayRegex();
        var match = regex.Match(type);
        if (!match.Success) return -1;
        return int.Parse(match.Groups[2].Value) - int.Parse(match.Groups[1].Value) + 1;
    }
    
    private static List<string> GetFullNamePath(XElement? element)
    {
        
        var namePath = new List<string>();
        if (element is null) return namePath;
            
        while (true)
        {
            //Not valid anymore
            if (element is null)
                return namePath;
            //Top device -> we are done
            if (element.Name == "Device")
                return namePath;
            //Element has element 'Name'           
            if (element.Element("Name") is not null)
                namePath.Insert(0, element.Element("Name")?.Value ?? "");
            //Element has attribute 'Name'  
            else if (element.Attribute("Name") is not null)
                namePath.Insert(0, element.Attribute("Name")?.Value ?? "");
            //Set to parent hierarchy
            element = element.Parent;
        }
    }

    [GeneratedRegex(@"ARRAY\s*\[(\d+)\.\.(\d+)\]\s+OF\s+(BYTE)", RegexOptions.IgnoreCase)]
    private static partial Regex ByteArrayRegex();
}