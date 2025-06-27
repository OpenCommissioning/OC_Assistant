using System.IO;
using System.Xml.Linq;
using EnvDTE;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.EtherCat;

/// <summary>
/// Represents a generator to parse an EtherCAT bus and create linked variables.
/// </summary>
internal class EtherCatGenerator
{
    private readonly DTE _dte;
    private readonly List<EtherCatInstance> _instance = [];
    private readonly string? _projectFolder;
    private readonly string _folderName;
        
    /// <summary>
    /// Instance of the <see cref="EtherCatGenerator"/>.
    /// </summary>
    /// <param name="dte">The <see cref="DTE"/> interface of the connected project.</param>
    /// <param name="folderName">The plc folder locating the created GVL(s).</param>
    public EtherCatGenerator(DTE dte, string folderName)
    {
        _dte = dte;
        _projectFolder = dte.GetProjectFolder();
        _folderName = folderName;
    }
        
    /// <summary>
    /// Parses EtherCAT bus node(s) and creates GVL(s) with linked variables.
    /// </summary>
    /// <param name="plcProjectItem">The <see cref="ITcSmTreeItem"/> of the plc project.</param>
    public void Generate(ITcSmTreeItem plcProjectItem)
    {
        var tcSysManager =_dte.GetTcSysManager();
        
        if (tcSysManager is null) return;
        
        //Get io area of project
        if (!tcSysManager.TryLookupTreeItem(TcShortcut.IO_DEVICE, out var ioItem))
        {
            return;
        }
        
        foreach (ITcSmTreeItem item in ioItem)
        {
            //Is not etherCat simulation
            if (item.ItemSubType != (int) TcSmTreeItemSubType.EtherCatSimulation) continue;
            
            //Is disabled
            if (item.Disabled == DISABLED_STATE.SMDS_DISABLED) continue;
            
            //Export etherCat simulation to xti file
            var name = item.Name.TcRemoveBrackets();
            var xtiFile = $"{AppData.Path}\\{name}.xti";
            if (File.Exists(xtiFile)) File.Delete(xtiFile);
            item.Parent.ExportChild(item.Name, xtiFile);
            
            //Parse etherCat and implement plc devices with tcLink attributes
            ParseXti(XDocument.Load(xtiFile));
            Implement(name, plcProjectItem);
            
            //Delete xti file
            File.Delete(xtiFile);
        }
    }
    
    private void ParseXti(XContainer xtiDocument)
    {
        var eCatName = xtiDocument.Element("TcSmItem")?.Element("Device")?.Attribute("RemoteName")?.Value.MakePlcCompatible();
        if (eCatName is null) return;
        
        _instance.Clear();
            
        var activeBoxes = (from elem in xtiDocument.Descendants("Box")
            where elem.Attribute("Disabled") is null
            select elem).ToList();
            
        var eCatCollection = TcEtherCatTemplates.ToList();
        var missingTypes = new List<string>();
            
        foreach (var box in activeBoxes)
        {
            var type = box.Element("EcatSimuBox")?.Attribute("Type")?.Value ?? "";
            if (IgnoreList.Any(ignoredType => ignoredType is not null && type.StartsWith(ignoredType))) continue;
            
            var nativeName = box.Element("Name")?.Value ?? "";
            var name = PlcCompatibleString(nativeName);
            var id = box.Attribute("Id")?.Value;
            var productCode = box.Element("EcatSimuBox")?.Element("BoxSettings")?.Attribute("ProductCode")?.Value;
            var template = eCatCollection.FirstOrDefault(x => type.StartsWith(x.ProductDescription));
            
            if (template is null)
            {
                foreach (var variable in new EtherCatVariables(name, box))
                {
                    _instance.Add(new EtherCatInstance(variable));
                }

                if (string.IsNullOrEmpty(type))
                {
                    type = $"unknown type {productCode}";
                }
                
                if (missingTypes.Any(x => x == type)) continue;
                missingTypes.Add(type);
                Logger.LogWarning(this, $"{nativeName}: Missing EtherCAT type in bus {eCatName}. {type} not found in any *.ethml file");
                continue;
            }
            
            _instance.Add(new EtherCatInstance(id, name, template));
        }
    }

    private void Implement(string name, ITcSmTreeItem plcProjectItem)
    {
        var hilFolder = plcProjectItem.GetOrCreateChild(_folderName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        var busFolder = hilFolder?.GetOrCreateChild(name, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        var prg = busFolder?.GetOrCreateChild($"PRG_{name}", TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG);
        
        var variables = _instance
            .Aggregate("", (current, next) => current + $"{next.DeclarationText}\n");

        busFolder.CreateGvl(name, variables);
        
        if (prg.CastTo<ITcPlcImplementation>() is {} impl)
        {
            impl.ImplementationText = _instance
                .Where(x => x.CyclicCall)
                .Aggregate("", (current, device) => current + $"GVL_{name}.{device.InstanceName}();\n");
        }
        
        XmlFile.Instance.Hil.Add(new XElement("Program", $"PRG_{name}".MakePlcCompatible()));
        XmlFile.Instance.Save();
    }
    
    private IEnumerable<EtherCatTemplate> TcEtherCatTemplates
    {
        get
        {
            return Directory.GetFiles($"{_projectFolder}", "*.ethml")
                .Select(XDocument.Load).SelectMany(doc => doc.Root?.Elements("Device")
                .Select(device => new EtherCatTemplate(device)) ?? new List<EtherCatTemplate>());
        }
    }
    
    private IEnumerable<string?> IgnoreList
    {
        get
        {
            return Directory.GetFiles($"{_projectFolder}", "*.ethml")
                .Select(XDocument.Load).SelectMany(doc => doc.Root?.Element("Ignore")?.Elements("Device")
                .Select(device => device.Attribute("ProductDescription")?.Value) ?? new List<string>());
        }
    }

    private static string PlcCompatibleString(string name)
    {
        //First remove the device type, e.g. KF2.1 (EL1008) => KF2.1
        var result = name.TcRemoveBrackets();

        //Then replace any special character
        return result.TcPlcCompatibleString();
    }
}