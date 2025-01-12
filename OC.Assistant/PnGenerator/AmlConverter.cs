using System.Xml.Linq;

namespace OC.Assistant.PnGenerator;

public static class AmlConverter
{
    /// <summary>
    /// Reads an aml file and converts to a readable <see cref="XElement"/>.
    /// </summary>
    /// <param name="amlFilePath">The aml file path.</param>
    /// <returns>The converted <see cref="XElement"/></returns>
    public static XElement? Read(string amlFilePath)
    {
        var aml = XDocument.Load(amlFilePath).Root;
        if (aml is null) return null;
        
        return new XElement("");
    }
}