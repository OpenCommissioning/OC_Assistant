using System.Xml.Linq;

namespace OC.Assistant.PnGenerator.Aml;

public class AmlConverter
{
    private int _currentDeviceOffset;
    private readonly Dictionary<string, string> _linkA = [];
    private readonly Dictionary<string, string> _linkB = [];
    private XElement? _aml;
    
    /// <summary>
    /// Reads an aml file and converts to a readable <see cref="XElement"/>.
    /// </summary>
    /// <param name="amlFilePath">The aml file path.</param>
    /// <returns>The converted <see cref="XElement"/></returns>
    public XElement? Read(string amlFilePath)
    {
        _linkA.Clear();
        _linkB.Clear();
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

        var typeIdentifiers = new HashSet<string>();
        foreach (var device in rootElement.Descendants("Device"))
        {
            var typeIdentifier = device.Attribute("TypeIdentifier")?.Value;
            if (typeIdentifier is null) continue;
            if (typeIdentifiers.Add(typeIdentifier))
            {
                rootElement.Add(new XElement("TypeIdentifier", 
                    new XAttribute("Name", typeIdentifier), 
                    new XElement("VendorId", 0), 
                    new XElement("DeviceId", 0)));
            }
        }
        
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
        var submodulesNo = parent is null ? 1 : parent.GetDeviceItems().Count();
        
        //Create module element (will be renamed as Submodule maybe later)
        var moduleElement = new XElement("Module" + (deviceItem.GetPositionNumber() + 1), 
            new XAttribute("Name", deviceItem.GetName() ?? "Unknown"));


        var portElement = new XElement("Ports");
        if (deviceElement.Element("Module1")?.Element(portElement.Name) is null)
        {
            GetNetworkInformation(deviceItem, portElement);
        }
            
        //Read address infos and check if this is a submodule
        var addresses = deviceItem.GetAddresses();
        var isSubModule = addresses is not null;
        var hasIOs = false;
        if (addresses is not null)
        {
            hasIOs = addresses.Aggregate(false, (current, address) => current | GetAddressAttributes(address, moduleElement));
            
            //Position number sometimes starts with 0, sometimes with 1
            if (submodulesNo > 1 || deviceItem.GetPositionNumber() == 0)
            {
                _currentDeviceOffset = 1;
            }
            moduleElement.Name = "Submodule" + (deviceItem.GetPositionNumber() + _currentDeviceOffset);
        }

        switch (isSubModule)
        {
            case true when hasIOs: //this is a submodule with IOs
            {
                //Read failsafe info
                if (deviceItem.IsProfisafeItem())
                {
                    moduleElement.Add(new XElement("FailsafeInfo", "#lp"));
                }
                
                //Because this is a submodule, add this element to a module
                var moduleNo = "1";
                    
                //Parent deviceItem exists -> should be the right module
                if (parent is not null)
                {
                    moduleNo = (parent.GetPositionNumber() + 1).ToString();
                }
                    
                //There is no parent module. Create new module with submodule number. New submodule number is 1
                if (parent is null)
                {
                    moduleElement.Name = "Submodule1";
                    moduleNo = (deviceItem.GetPositionNumber() + 1).ToString();
                }

                //Module does not exist yet -> create new
                if (!deviceElement.ChildExists($"Module{moduleNo}"))
                {
                    deviceElement.Add(new XElement($"Module{moduleNo}", new XAttribute("Name", deviceItem.Name)));
                }

                //Add submodule to module
                deviceElement.Element($"Module{moduleNo}")?.Add(moduleElement);
                break;
            }
            case false: //This is a module -> add to device directly
            {
                _currentDeviceOffset = 0;

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