using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OC.Assistant.Generator.EtherCat;

internal partial class EtherCatTemplate
{
    public string ProductDescription { get; }
    public string DeclarationTemplate { get; }
    public bool CyclicCall { get; }

    public EtherCatTemplate(XElement device)
    {
        ProductDescription = device.Attribute("ProductDescription")?.Value ?? "";
        
        DeclarationTemplate = LeadingWhitespace()
            .Replace(device.Element("Declaration")?.Value ?? "", "");
        
        CyclicCall = FunctionBlock().Match(DeclarationTemplate).Success;
    }

    [GeneratedRegex(@"^[\t ]+", RegexOptions.Multiline)]
    private static partial Regex LeadingWhitespace();
    
    [GeneratedRegex(@"\$NAME\$\s*:\s*FB_\w+")]
    private static partial Regex FunctionBlock();
}