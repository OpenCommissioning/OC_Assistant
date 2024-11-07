using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OC.Assistant.Generator.EtherCat;

internal class EtherCatTemplate
{
    public string ProductDescription { get; }
    public string DeclarationTemplate { get; }
    public string MappingTemplate { get; }
    public bool CyclicCall { get; }

    public EtherCatTemplate(XElement device)
    {
        ProductDescription = device.Attribute("ProductDescription")?.Value ?? "";
        DeclarationTemplate = device.Element("Declaration")?.Value ?? "";
        MappingTemplate = device.Element("Mapping")?.Value ?? "";
        CyclicCall = Regex.Match(DeclarationTemplate, @"\$NAME\$\s*:\s*FB_\w+").Success;
    }
}