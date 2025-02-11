using System.Text.RegularExpressions;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator.Profinet;

internal class ProfinetVariable
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
    /// "I" or "Q"
    /// </summary>
    public string Direction { get; }
        
    public ProfinetVariable(XElement? element, string pnName, bool isInput)
    {
        var nameList = GetFullNamePath(element);
            
        Direction = isInput ? "I" : "Q";
        Link = nameList
            .Aggregate($"{TcShortcut.IO_DEVICE}^{pnName}", (current, next) => $"{current}^{next}");
        Type = element?.Element("Type")?.Value ?? TcType.Byte.Name();
        
        Name = nameList[^1];
        
        if (NameIsAddress) return;

        Name = nameList.Where(x => x != "API" && x != "Inputs" && x != "Outputs")
            .Aggregate("", (current, next) => $"{current}_{next.TcRemoveBrackets()}")
            .TcPlcCompatibleString();
    }

    /// <summary>
    /// Check if name meets the address pattern, e.g. I100 or Q100
    /// </summary>
    private bool NameIsAddress => Regex.Match(Name, @"^[I,Q]\d+$").Success;

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
}