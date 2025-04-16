using System.Xml.Linq;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.PnGenerator.Aml;

public class AmlConverter
{
    private readonly Dictionary<string, string> _linkA = [];
    private readonly Dictionary<string, string> _linkB = [];
    private XElement? _aml;
    
    /// <summary>
    /// Reads an TIA aml export file and converts to a simplified <see cref="XElement"/>.
    /// </summary>
    /// <param name="amlFilePath">The aml file path.</param>
    /// <returns>The converted <see cref="XElement"/></returns>
    public XElement? Read(string? amlFilePath)
    {
        _linkA.Clear();
        _linkB.Clear();
        if (amlFilePath is null) return null;
        _aml = XDocument.Load(amlFilePath).Root;
        var rootElement = new XElement("Profinet");
        if (_aml is null) return null;
        
        GetLinks();

        foreach (var device in _aml.GetDevices())
        {
            if (device is null) continue;
            var name = device.GetPnDeviceNameConverted();
            if (name is null) continue;
            var deviceElement = new XElement("Device", new XAttribute("Name", name));
            foreach (var deviceItem in device.GetDeviceItems())
            {
                GetDeviceItemInformation(deviceItem, null, deviceElement);
            }
                
            rootElement.Add(deviceElement);
        }
        
        CreateDeviceFile(rootElement);
        return rootElement;
    }
    
    private void GetDeviceItemInformation(XElement deviceItem, XElement? parent, XElement deviceElement)
    {
        GetDeviceItemAttributes(deviceItem, parent, deviceElement);
        
        //Call recursive
        foreach (var deviceSubItem in deviceItem.GetDeviceItems())
        {
            GetDeviceItemInformation(deviceSubItem, deviceItem, deviceElement);
        }
    }

    private void GetDeviceItemAttributes(XElement deviceItem, XElement? parent, XElement deviceElement)
    {
        var moduleElement = new XElement($"Module{deviceItem.GetPositionNumber() + 1}");
        
        var portElement = new XElement("Ports");
        if (deviceElement.Element("Module1")?.Element(portElement.Name) is null)
        {
            GetNetworkInformation(deviceItem, portElement);
        }
            
        //Read address infos and check if this is a submodule
        var addresses = deviceItem.GetAddresses();
        var isSubModule = addresses is not null && parent is not null;
        var hasIOs = addresses?.Aggregate(false, (current, address) => current | GetAddressAttributes(address, moduleElement)) == true;

        switch (isSubModule)
        {
            case true when hasIOs: //this is a submodule with IOs
            {
                //Read failsafe info
                if (deviceItem.IsProfisafeItem())
                {
                    moduleElement.Add(new XAttribute("IsFailsafe", true));
                }
                
                //Because this is a submodule, add this element to a module
                string moduleName;
                
                if (parent?.GetPositionNumber() == 0)
                {
                    moduleElement.Name = "Submodule1";
                    moduleName = $"Module{deviceItem.GetPositionNumber() + 1}";
                }
                else
                {
                    var pos = deviceItem.GetPositionNumber();
                    if (pos == 0) pos = 1;
                    moduleElement.Name = $"Submodule{pos}";
                    moduleName = $"Module{parent?.GetPositionNumber() + 1}";
                }

                //Module does not exist yet -> create new
                if (!deviceElement.ChildExists(moduleName))
                {
                    deviceElement.Add(new XElement(moduleName));
                }

                //Add submodule to module
                deviceElement.Element(moduleName)?.Add(moduleElement);
                break;
            }
            case false: //This is a module -> add to device directly
            {
                if (deviceElement.ChildExists(moduleElement.Name))
                {
                    foreach (var item in moduleElement.Elements())
                    {
                        deviceElement.Element(moduleElement.Name)?.Add(item);
                    }
                }
                else
                {
                    deviceElement.Add(moduleElement);
                }
                
                var typeIdentifier = deviceItem.GetAttributeValue("TypeIdentifier");
                if (typeIdentifier is not null && !deviceElement.AttributeExists("TypeIdentifier"))
                {
                    if (typeIdentifier.Contains("OrderNumber:") || typeIdentifier.Contains("GSD:"))
                    {
                        deviceElement.Add(new XAttribute("TypeIdentifier", typeIdentifier));
                    }
                }
                break;
            }
        }

        //Add ports to module if available
        if (!portElement.HasElements) return;
        if (!deviceElement.ChildExists("Module1"))
        {
            deviceElement.Add(new XElement("Module1"));
        }
        deviceElement.Element("Module1")?.Add(portElement);
    }

    private static void CreateDeviceFile(XElement rootElement)
    {
        var jsonFile = new Dictionary<string, string>();
        var deviceIds = GetDeviceIdsFromGit();
        var missing = new HashSet<string>();
        
        foreach (var device in rootElement.Descendants("Device"))
        {
            if (device.Attribute("Name")?.Value is not {} name) continue;
            if (device.Attribute("TypeIdentifier")?.Value is not {} typeIdentifier) continue;
            typeIdentifier = typeIdentifier.Split('/')[0];
            
            if (!deviceIds.TryGetValue(typeIdentifier, out var deviceId))
            {
                if (missing.Add(typeIdentifier))
                {
                    Logger.LogWarning(typeof(AmlConverter),$"Unknown device of type {typeIdentifier}");
                }
                continue;
            }
            jsonFile.Add(name, deviceId);
        }

        if (JsonSerializer.Serialize(jsonFile) is not {} jsonString) return;
        File.WriteAllText($"{AppData.Path}\\DeviceIds.json", jsonString);
    }

    private static Dictionary<string, string> GetDeviceIdsFromGit()
    {
        const string url = "https://raw.githubusercontent.com/opencommissioning/OC_ProfinetDeviceIds/main/DeviceIds.json";
        using var client = new HttpClient();
        try
        {
            var raw = client.GetStringAsync(url).Result;
            return JsonSerializer.Deserialize<Dictionary<string, string>>(raw) ?? new Dictionary<string, string>();
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(AmlConverter), e.Message);
            return new Dictionary<string, string>();
        }
    }

    private static bool GetAddressAttributes(XElement address, XElement moduleElement)
    {
        var element = new XElement("Addresses");
        var ioType = address.GetAttributeValue("IoType");
        var hasIOs = false;
        if (ioType is "Input" or "Output")
        {
            element.Add(new XElement("IoType", ioType));
            element.Add(new XElement("StartAddress", address.GetAttributeValue("StartAddress")));
            element.Add(new XElement("Length", address.GetAttributeValue("Length")));
            hasIOs = true;
        }

        moduleElement.Add(element);
        return hasIOs;
    }
    
    private void GetNetworkInformation(XElement deviceItem, XElement portElement)
    {
        foreach (var port in deviceItem.GetCommunicationPorts())
        {
            var id = port?.GetId();
            if (id is null) continue;

            var linkedPort = GetLinkedPort(id);
            if (linkedPort is null) continue;

            var linkedDeviceName = linkedPort.GetPnDeviceNameByPort();
            if (linkedDeviceName is null) continue;
            
            var connectedPortNr = ".port-" + linkedPort.GetPositionNumber().ToString("000");
            
            var elem = new XElement("Port" + port?.GetPositionNumber());
            elem.Add(new XAttribute("RemPeerPort",linkedDeviceName + connectedPortNr));
            portElement.Add(elem);
        }
    }

    private XElement? GetLinkedPort(string id)
    {
        if (_aml is null) return null;
        
        if (!_linkA.TryGetValue(id, out var linkedId))
        {
            if (!_linkB.TryGetValue(id, out linkedId))
            {
                return null;
            }
        }
        
        return _aml
            .Descendants("InternalElement")
            .FirstOrDefault(x => x.Attribute("ID")?.Value == linkedId);
    }
    
    private void GetLinks()
    {
        if (_aml is null) return;
        
        var portLinks = _aml
            .Descendants("InternalLink")
            .Where(x => x.GetName()?.Contains("Link To Port") == true);
        
        foreach (var portLink in portLinks)
        {
            var linkA = portLink.Attribute("RefPartnerSideA")?.Value;
            var linkB = portLink.Attribute("RefPartnerSideB")?.Value;
            if (linkA is null || linkB is null) continue;
            _linkA.Add(linkA.Split(':')[0], linkB.Split(':')[0]);
            _linkB.Add(linkB.Split(':')[0], linkA.Split(':')[0]);
        }
    }
}