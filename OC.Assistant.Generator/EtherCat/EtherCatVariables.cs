using System.Xml.Linq;

namespace OC.Assistant.Generator.EtherCat;

/// <summary>
/// Represents an extended list of type <see cref="EtherCatVariable"/>.
/// </summary>
internal class EtherCatVariables : List<EtherCatVariable>
{
    private readonly List<string> _uniquePdoNames = [];
    
    /// <summary>
    /// Parses an EtherCAT box into a list of <see cref="EtherCatVariable"/>.
    /// </summary>
    /// <param name="name">The managed (plc compatible) name of the EtherCAT box.</param>
    /// <param name="box"><see cref="XElement"/> representing an EtherCAT box.</param>
    public EtherCatVariables(string name, XElement box)
    {
        ParseBox(name, box);
    }

    private void ParseBox(string boxName, XElement box)
    {
        var id = box.Attribute("Id")?.Value;
        var pdoNodes = box.Descendants("Pdo")
            .Where(pdo => !string.IsNullOrEmpty(pdo.Attribute("SyncMan")?.Value));

        foreach (var pdoNode in pdoNodes)
        {
            var pdoName = pdoNode.Attribute("Name")?.Value ?? "";
            var uniquePdoName = GetUniquePdoName(pdoName);
            var entryNodes = pdoNode.Descendants("Entry");
            var syncMan = pdoNode.Attribute("SyncMan")?.Value;

            var inOut = syncMan switch
            {
                "2" => "AT %I*",
                "3" => "AT %Q*",
                _ => null
            };
            
            if (inOut is null) continue;

            foreach (var entryNode in entryNodes.Where(x => !string.IsNullOrEmpty(x.Attribute("Index")?.Value)))
            {
                var nativeName = entryNode.Attribute("Name")?.Value ?? "";
                
                // Two underscores in a row usually means the entry is part of a structure
                // e.g. 'StructName__StructName EntryName'
                // -> we only need the substring starting at the end of the underscores
                var index = nativeName.IndexOf("__", StringComparison.Ordinal);
                var cleanedName = index > -1 ? nativeName[index..] : nativeName;
                
                //Shorten the name by replacing In- and Output with I and Q
                cleanedName = cleanedName.Replace("Output ", "Q").Replace("Input ", "I");
                
                var name = $"{boxName}_{uniquePdoName}_{cleanedName}";
                var type = $"{inOut} : {entryNode.Element("Type")?.Value}";
                var linkTo = $"{Core.TwinCat.TcShortcut.BOX}({id})^{uniquePdoName}^{nativeName.Replace("__", "^")}";

                Add(new EtherCatVariable(name, type, linkTo));
            }
        }
    }

    private string GetUniquePdoName(string originalName)
    {
        var modifiedName = originalName;
        var counter = 1;
        
        while (_uniquePdoNames.Contains(modifiedName))
        {
            modifiedName = $"{originalName}_{counter}";
            counter++;
        }

        _uniquePdoNames.Add(modifiedName);
        return modifiedName;
    }
}