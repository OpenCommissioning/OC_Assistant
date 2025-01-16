using System.Text;
using System.Xml.Linq;

namespace OC.Assistant.PnGenerator.Aml;

public static class XElementExtension
{
    public static bool ChildExists(this XElement element, XName childName)
    {
        return element.Element(childName) is not null;
    }

    public static bool AttributeExists(this XElement element, XName attributeName)
    {
        return element.Attribute(attributeName) is not null;
    }

    public static string? GetPnDeviceNameConverted(this XElement element)
    {
        var name = element
            .Descendants("Attribute")
            .Where(x => x.Attribute("Name")?.Value == "ProfinetDeviceName")
            .Select(x => x.Element("Value")?.Value)
            .FirstOrDefault();
        
        var convertedName = ConvertToPnString(name);
        return convertedName is null ? null : $"{convertedName}{Crc16Arc.Calculate(convertedName):x4}";
    }
    
    public static string? GetPnDeviceNameByPort(this XElement portElement)
    {
        return portElement.Parent?.GetEthernetNode()?.GetPnDeviceNameConverted();
    }
    
    private static string? ConvertToPnString(string? value)
    {
        if (value is null) return null;
        
        var charMappings = new Dictionary<char, string>
        {
            { '=', "xv" },
            { '+', "xn" },
            { '_', "xb" },
            { ' ', "xa" },
            { 'x', "xx" }
        };
        
        var result = new StringBuilder();

        foreach (var c in value)
        {
            if (charMappings.TryGetValue(c, out var mapping))
            {
                result.Append(mapping);
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
    
    public static IEnumerable<XElement?> GetDevices(this XElement element)
    {
        return element
            .Descendants("SupportedRoleClass")
            .Where(x => x.Attribute("RefRoleClassPath")?.Value == "AutomationProjectConfigurationRoleClassLib/Device")
            .Select(x => x.Parent);
    }

    private static XElement? GetEthernetNode(this XElement element)
    {
        return element
            .Descendants("SupportedRoleClass")
            .FirstOrDefault(x => x.Attribute("RefRoleClassPath")?.Value == "AutomationProjectConfigurationRoleClassLib/Node")?
            .Parent;
    }
    
    public static IEnumerable<XElement> GetDeviceItems(this XElement element)
    {
        return element
            .Elements("InternalElement")
            .Where(x => x.Element("SupportedRoleClass")?.Attribute("RefRoleClassPath")?.Value == "AutomationProjectConfigurationRoleClassLib/DeviceItem");
    }
    
    public static bool IsProfisafeItem(this XElement element)
    {
        return element
            .Elements("SupportedRoleClass")
            .FirstOrDefault(x => x.Attribute("RefRoleClassPath")?.Value == "AutomationProjectConfigurationProfiSafeRoleClassLib/DeviceItemProfiSafe") is not null;
    }
    
    public static IEnumerable<XElement?> GetCommunicationPorts(this XElement element)
    {
        return element
            .Descendants("SupportedRoleClass")
            .Where(x => x.Attribute("RefRoleClassPath")?.Value == "AutomationProjectConfigurationRoleClassLib/CommunicationPort")
            .Select(x => x.Parent);
    }
    
    public static string? GetName(this XElement element)
    {
        return element.Attribute("Name")?.Value;
    }
    
    public static string? GetId(this XElement element)
    {
        return element.Attribute("ID")?.Value;
    }
    
    public static int GetPositionNumber(this XElement element)
    {
        var number = element.GetAttributeValue("PositionNumber");
        if (number is null) return -1;
        
        if (int.TryParse(number, out var result))
        {
            return result;
        }

        return -1;
    }

    public static IEnumerable<XElement>? GetAddresses(this XElement element)
    {
        return element
            .Elements("Attribute")
            .FirstOrDefault(x => x.Attribute("Name")?.Value == "Address")?
            .Elements("Attribute");
    }
    
    public static string? GetAttributeValue(this XElement element, string attributeName)
    {
        return element
            .Elements("Attribute")
            .FirstOrDefault(x => x.Attribute("Name")?.Value == attributeName)?
            .Element("Value")?.Value;
    }

    public static bool IsHeadModule(this XElement element)
    {
        return element
            .Elements("Attribute")
            .FirstOrDefault(x => x.Attribute("Name")?.Value == "DeviceItemType")?
            .Element("Value")?.Value == "HeadModule";
    }
}