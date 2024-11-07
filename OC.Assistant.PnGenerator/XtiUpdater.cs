using System.Xml.Linq;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator;

internal class XtiUpdater
{
    private XElement? _hwFile;
    private List<string> _deviceNames = [];
        
    public void Run(string xtiFilePath, string hwFilePath)
    {
        try
        {
            _hwFile = XDocument.Load(hwFilePath).Root;
            if (_hwFile is null) return;
            
            Logger.LogInfo(this, $"Updating {xtiFilePath} with {hwFilePath}");
            var xti = XDocument.Load(xtiFilePath);
            
            xti.Root!.Element("Device")!.Element("Profinet")!.Attribute("PLCPortNr")!.Value = "852"; 
            
            var xContainers = xti.Descendants("Box");
            var boxes = xContainers.ToList();
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

    private void UpdateBox(XContainer box)
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

    private void UpdateModule(XContainer module, int moduleIndex, string boxName)
    {
        var submoduleIndex = 1;
        foreach (var subModule in module.Elements("SubModule"))
        {
            UpdateSubModule(subModule, submoduleIndex, moduleIndex, boxName);
            submoduleIndex++;
        }
    }
        
    private void UpdateSubModule(XElement subModule, int submoduleIndex, int moduleIndex, string boxName)
    {
        var subSlotNumber = subModule.Attribute("SubSlotNumber")?.Value;
        if (subSlotNumber is null) return;
            
        var typeOfSubmodule = GetTypeOfSubmodule(subSlotNumber);
        var remPeerPort = GetRemPeerPort(typeOfSubmodule, boxName, submoduleIndex, moduleIndex);
            
        subModule.Add(new XAttribute("TypeOfSubModule", typeOfSubmodule));
            
        if (typeOfSubmodule == "2")
        {
            subModule.Add(new XAttribute("PortData", "00000000000000000000000000000000000000000000000000000000"));
            if (remPeerPort != "") subModule.Add(new XAttribute("RemPeerPort", remPeerPort));
        }
            
        var matchedSubmodule = GetMatchedSubmodule(boxName, moduleIndex, submoduleIndex);
        if (matchedSubmodule is null) return;

        var name = subModule.Element("Name")?.Value;
        if (name is not null)
        {
            name += GetFailsafeInfo(matchedSubmodule);
            subModule.Element("Name")!.Value = name;
        }

        var inputVar = subModule
            .Elements("Vars")
            .FirstOrDefault(x => x.Attribute("VarGrpType")?.Value == "1");
            
        var outputVar = subModule
            .Elements("Vars")
            .FirstOrDefault(x => x.Attribute("VarGrpType")?.Value == "2");

        if (inputVar != default) SetAddress(inputVar, matchedSubmodule, true);
        if (outputVar != default) SetAddress(outputVar, matchedSubmodule, false);
    }

    private static void SetAddress(XContainer? var, XContainer matchedSubmodule, bool isInput)
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

    private string GetRemPeerPort(string typeOfSubmodule, string boxName, int submoduleIndex, int moduleIndex)
    {
        if (typeOfSubmodule != "2") return "";
            
        try
        {
            var remPeerPort = _hwFile?
                .Elements("Device")
                .First(x => string.Equals(x.Attribute("Name")?.Value, boxName, StringComparison.CurrentCultureIgnoreCase))
                .Element("Module" + moduleIndex)
                !.Element("Ports")
                !.Element("Port" + (submoduleIndex - 2))
                !.Attribute("RemPeerPort")
                !.Value;

            var remPeerPort0 = remPeerPort?.Split('.')[0];

            return _deviceNames.Any(x => remPeerPort0 is not null && x == remPeerPort0) ? remPeerPort ?? "" : "";
        }
        catch
        {
            return "";
        }
    }

    private XElement? GetMatchedSubmodule(string boxName, int moduleIndex, int submoduleIndex)
    {
        try
        {
            return _hwFile?
                .Elements("Device")
                .First(x => string.Equals(x.Attribute("Name")?.Value, boxName, StringComparison.CurrentCultureIgnoreCase))
                .Element($"Module{moduleIndex}")
                ?.Element($"Submodule{submoduleIndex}");
        }
        catch
        {
            return null;
        }
    }
        
    private static int GetStartAddress(XContainer matchedSubmodule, string filter)
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

    private static string GetFailsafeInfo(XContainer matchedSubmodule)
    {
        return $" {matchedSubmodule.Element("FailsafeInfo")?.Value ?? ""}";
    }
}