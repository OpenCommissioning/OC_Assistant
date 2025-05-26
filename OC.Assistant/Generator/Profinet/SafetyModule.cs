using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator.Profinet;

internal class SafetyModule
{
    public int Port { get; }
    public int Slot { get; }
    public int SubSlot { get; }
    public string Name { get; }
    public string BoxName { get; }
    public string PnName { get; }
    public string HstDataName { get; } = "";
    public string DevDataName { get; } = "";
    public int HstSize { get; }
    public int DevSize { get; }
    
    public ProfinetVariable? HstVariable { get; set; }
    public ProfinetVariable? DevVariable { get; set; }

    public bool IsValid => HstSize >= 0 && DevSize >= 0;
        
    public SafetyModule (XElement? element, string pnName)
    {
        Port = IsFirstBox(element) ? 65535 : GetPortNr(element);
        BoxName = GetBoxName(element);
        PnName = pnName;
        Slot = GetSlotNr(element?.Parent);
        SubSlot = GetSubSlotNr(element);
        Name = $"fb{Port}x{Slot}x{SubSlot}";

        var inputs = new List<ProfinetVariable>();
        var outputs = new List<ProfinetVariable>();

        if (element is null) return;

        foreach (var elem in element.Descendants("BitOffs"))
        {
            if (elem.Parent?.Parent?.Attribute("VarGrpType")?.Value == "1")
            {
                inputs.Add(new ProfinetVariable(elem.Parent, pnName, true));
            }

            if (elem.Parent?.Parent?.Attribute("VarGrpType")?.Value == "2")
            {
                outputs.Add(new ProfinetVariable(elem.Parent, pnName, false));
            }
        }

        if (inputs.Count == 0 || outputs.Count == 0) return;

        foreach (var input in inputs)
        {
            HstSize += input.Type.TcBitSize() / 8;
        }
            
        foreach (var output in outputs)
        {
            DevSize += output.Type.TcBitSize() / 8;
        }

        HstDataName = inputs[0].Name;
        DevDataName = outputs[0].Name;
    }

    /// <summary>
    /// Determines, if the current Box is the first one
    /// </summary>
    private static bool IsFirstBox(XObject? element)
    {
        var parent = element?.Parent;
        if (parent?.Name != "Box")
        {
            return IsFirstBox(element?.Parent);
        }
        var previous = parent.PreviousNode as XElement;
        return previous?.Name != "Box";
    }

    /// <summary>
    /// Search recursively upwards
    /// </summary>
    private static string GetBoxName(XObject? element)
    {
        if (element?.Parent?.Name == "Box") return element.Parent?.Element("Name")?.Value ?? "";
        return GetBoxName(element?.Parent);
    }

    /// <summary>
    /// Search recursively upwards until Element 'Box'
    /// </summary>
    private static int GetPortNr(XElement? element)
    {
        if (element?.Name == "Box") return int.Parse(element.Attribute("Id")?.Value ?? "0") + 0x1000;
        return GetPortNr(element?.Parent);
    }

    /// <summary>
    /// Search recursively upwards until Element !'Module'
    /// </summary>
    private static int GetSlotNr(XNode? element)
    {
        var elem = element?.PreviousNode as XElement;
        if (elem?.Name != "Module") return 0;
        return GetSlotNr(elem) + 1;
    }

    /// <summary>
    /// Search recursively upwards until Element !'SubModule'
    /// </summary>
    private static int GetSubSlotNr(XNode? element)
    {
        var elem = element?.PreviousNode as XElement;
        if (elem?.Name != "SubModule") return 1;
        return GetSubSlotNr(elem) + 1;
    }
}