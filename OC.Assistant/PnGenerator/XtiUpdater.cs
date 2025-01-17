using System.Xml.Linq;
using OC.Assistant.PnGenerator.Aml;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

internal class XtiUpdater
{
    private XElement? _amlConverted;
    private List<string> _deviceNames = [];
        
    public void Run(string xtiFilePath, string amlFilePath)
    {
        try
        {
            _deviceNames.Clear();
            _amlConverted = new AmlConverter().Read(amlFilePath);
            
            if (_amlConverted is null)
            {
                Logger.LogError(this, "Error reading aml file");
                return;
            }
            
            var xti = XDocument.Load(xtiFilePath).Root;
            if (xti is null)
            {
                Logger.LogError(this, "Error reading xti file");
                return;
            }
            
            Logger.LogInfo(this, $"Updating {xtiFilePath} with {amlFilePath}");
            
            var plcPortNr = xti.Element("Device")?.Element("Profinet")?.Attribute("PLCPortNr");
            if (plcPortNr is not null)
            {
                plcPortNr.Value = "852"; 
            }
            
            var boxes = xti.Descendants("Box").ToList();
            if (boxes.Count == 0) return;
            
            _deviceNames = (from box in boxes select box.Element("Name")?.Value).ToList();
            
            foreach (var box in boxes)
            {
                UpdateBox(box);
            }
            
            xti.Save(xtiFilePath);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }

    private void UpdateBox(XElement box)
    {
        var boxName = box.Element("Name")?.Value;
        if (boxName is null) return;
            
        var modules = box.Element("Profinet")?.Element("API")?.Elements("Module");
        if (modules is null) return;

        var moduleIndex = 1;
        foreach (var module in modules)
        {
            UpdateModule(module, moduleIndex, boxName);
            moduleIndex++;
        }
    }

    private void UpdateModule(XElement module, int moduleIndex, string boxName)
    {
        var submoduleIndex = 1;
        foreach (var subModule in module.Elements("SubModule"))
        {
            UpdateSubModule(subModule, submoduleIndex, moduleIndex, boxName);
            submoduleIndex++;
        }
    }

    private static void SetAttribute(XElement element, string attributeName, string attributeValue)
    {
        var attribute = element.Attribute(attributeName);
        if (attribute is null)
        {
            element.Add(new XAttribute(attributeName, attributeValue));
            return;
        }
        attribute.Value = attributeValue;
    }
        
    private void UpdateSubModule(XElement subModule, int submoduleIndex, int moduleIndex, string boxName)
    {
        var subSlotNumber = subModule.Attribute("SubSlotNumber")?.Value;
        if (subSlotNumber is null) return;
            
        var typeOfSubmodule = GetTypeOfSubmodule(subSlotNumber);
        var remPeerPort = GetRemPeerPort(typeOfSubmodule, boxName, submoduleIndex, moduleIndex);
        
        SetAttribute(subModule, "TypeOfSubModule", typeOfSubmodule);
            
        if (typeOfSubmodule == "2")
        {
            SetAttribute(subModule, "PortData", "00000000000000000000000000000000000000000000000000000000");
            if (remPeerPort is not null)
            {
                SetAttribute(subModule, "RemPeerPort", remPeerPort);
            }
        }
            
        var matchedSubmodule = GetMatchedSubmodule(boxName, moduleIndex, submoduleIndex);
        if (matchedSubmodule is null) return;

        var name = subModule.Element("Name")?.Value;
        if (name is not null)
        {
            if (IsFailsafe(matchedSubmodule))
            {
                name += " #failsafe";
            }
            subModule.Element("Name")!.Value = name;
        }

        var inputVar = subModule
            .Elements("Vars")
            .FirstOrDefault(x => x.Attribute("VarGrpType")?.Value == "1");
            
        var outputVar = subModule
            .Elements("Vars")
            .FirstOrDefault(x => x.Attribute("VarGrpType")?.Value == "2");

        if (inputVar != null) SetAddress(inputVar, matchedSubmodule, true);
        if (outputVar != null) SetAddress(outputVar, matchedSubmodule, false);
    }

    private static void SetAddress(XElement? var, XElement matchedSubmodule, bool isInput)
    {
        var name = var?.Element("Var")?.Element("Name");
        if (name is null) return;
            
        var address = GetStartAddress(matchedSubmodule, isInput ? "Output" : "Input");
        if (address == -1) return;
            
        name.Value = isInput ? $"Q{address}" : $"I{address}";
    }


    private static string GetTypeOfSubmodule(string subSlotNumber) 
    {
        var subSlot = int.Parse(subSlotNumber);

        if (subSlot is >= 0x8000 and <= 0x8FFF)
        {
            return subSlot % 0x100 == 0 ? "1" : "2";
        }

        return "3";
    }

    private XElement? GetMatchedDevice(string boxName)
    {
        try
        {
            return _amlConverted?
                .Descendants("Device")
                .FirstOrDefault(x =>
                    string.Equals(x.Attribute("Name")?.Value, boxName, StringComparison.CurrentCultureIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    private string? GetRemPeerPort(string typeOfSubmodule, string boxName, int submoduleIndex, int moduleIndex)
    {
        if (typeOfSubmodule != "2") return null;
            
        try
        {
            var remPeerPort = GetMatchedDevice(boxName)?
                .Element($"Module{moduleIndex}")?
                .Element("Ports")?
                .Element("Port" + (submoduleIndex - 2))?
                .Attribute("RemPeerPort")?
                .Value;
            
            return _deviceNames.Any(x => x == remPeerPort?.Split('.')[0]) ? remPeerPort : null;
        }
        catch
        {
            return null;
        }
    }

    private XElement? GetMatchedSubmodule(string boxName, int moduleIndex, int submoduleIndex)
    {
        try
        {
            return GetMatchedDevice(boxName)?
                .Element($"Module{moduleIndex}")?
                .Element($"Submodule{submoduleIndex}");
        }
        catch
        {
            return null;
        }
    }
        
    private static int GetStartAddress(XElement matchedSubmodule, string filter)
    {
        var addresses = matchedSubmodule.Elements("Addresses");

        foreach (var address in addresses)
        {
            var ioType = address.Element("IoType")?.Value;
            if (ioType is null) continue;
            if (address.Element("IoType")?.Value != filter) continue;
            return int.Parse(address.Element("StartAddress")?.Value ?? "-1");
        }
            
        return -1;
    }

    private static bool IsFailsafe(XElement matchedSubmodule)
    {
        return bool.TryParse(matchedSubmodule.Attribute("IsFailsafe")?.Value, out var result) && result;
    }
}