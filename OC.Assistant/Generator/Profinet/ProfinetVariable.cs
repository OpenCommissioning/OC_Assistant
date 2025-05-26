using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator.Profinet;

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
            .Aggregate($"{TcShortcut.IO_DEVICE}^{pnName}", (current, next) => $"{current}^{next}");
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
            .MakePlcCompatible();
    }


    /// <summary>
    /// Generates a declaration string for a Profinet variable.
    /// </summary>
    /// <param name="tab">Indicates whether to use a tab character for formatting the declaration.</param>
    /// <param name="link">Indicates whether to include a link attribute in the declaration.</param>
    /// <returns>A formatted string representing the declaration of the Profinet variable.</returns>
    public string CreateDeclaration(bool tab, bool link)
    {
        var tabString = tab ? "\t" : "";
        var linkString = link ? $"{tabString}{{attribute 'TcLinkTo' := '$LINK$'}}\n" : "";
        var template = $"{linkString}{tabString}$VARNAME$ AT %$DIRECTION$* : $VARTYPE$;\n";
        
        if (this is not {PlcAddress.IsValid: true, ByteArraySize: >= 0})
            return template
                .Replace(Tags.VAR_TYPE, Type)
                .Replace(Tags.DIRECTION, Direction)
                .Replace(Tags.LINK, Link)
                .Replace(Tags.VAR_NAME, $"{Name}");
        
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
    
    private static int GetByteArraySize(string type)
    {
        var regex = ByteArrayRegex();
        var match = regex.Match(type);
        if (!match.Success) return -1;
        return int.Parse(match.Groups[2].Value) - int.Parse(match.Groups[1].Value) + 1;
    }

    /// <summary>
    /// Build Name recursively upwards until Element 'Device'
    /// </summary>
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