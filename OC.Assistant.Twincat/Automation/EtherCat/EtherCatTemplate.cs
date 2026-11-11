using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OC.Assistant.Twincat.Automation.EtherCat;

internal partial class EtherCatTemplate
{
    public string ProductDescription { get; }
    public string DeclarationTemplate { get; }
    public IReadOnlyList<string> CyclicCallSuffixes { get; }

    public EtherCatTemplate(XElement device)
    {
        ProductDescription = device.Attribute("ProductDescription")?.Value ?? "";
        
        DeclarationTemplate = LeadingWhitespace()
            .Replace(device.Element("Declaration")?.Value ?? "", "");
        
        CyclicCallSuffixes = FunctionBlock().Matches(DeclarationTemplate)
            .Select(match => match.Groups["suffix"].Value)
            .ToArray();
    }

    [GeneratedRegex(@"^[\t ]+", RegexOptions.Multiline)]
    private static partial Regex LeadingWhitespace();
    
    [GeneratedRegex(@"\$NAME\$\s*:\s*FB_\w+(?:_(?<suffix>\w+))?")]
    private static partial Regex FunctionBlock();
}