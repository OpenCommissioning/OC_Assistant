using System.Xml.Linq;
using OC.Assistant.Core;

namespace OC.Assistant.Generator;

/// <summary>
/// <see cref="Core.XmlFile"/> extension to write and read HiL configurations.
/// </summary>
internal static class XmlFile
{
    public static Core.XmlFile XmlBase => Core.XmlFile.Instance;
        
    /// <summary>
    /// Gets the Main program.
    /// </summary>
    public static XElement? Main => XmlBase.Project?.Element(XmlTags.MAIN);
    
    /// <summary>
    /// Implements a new client configuration.
    /// </summary>
    public static void ClientUpdate(XElement config)
    {
        try
        {
            var main = XmlBase.Project?.Element(XmlTags.MAIN);
            if (main is null) return;
            main.ReplaceNodes(config.Elements());
            XmlBase.Save();
        }
        catch (Exception e)
        {
            Sdk.Logger.LogWarning(nameof(XmlFile), e.Message);
        }
    }
        
    /// <summary>
    /// Gets the HiL programs.
    /// </summary>
    public static IEnumerable<string>? HilPrograms
    {
        get
        {
            return XmlBase.Project?.Element(XmlTags.HIL)?.Elements().Select(x => x.Name.ToString());
        }
    }

    public static IEnumerable<XElement>? PluginElements => XmlBase.Plugins?.Elements(XmlTags.PLUGIN);

    /// <summary>
    /// Removes all HiL programs.
    /// </summary>
    public static void ClearHilPrograms()
    {
        XmlBase.Project?.Element(XmlTags.HIL)?.RemoveAll();
        XmlBase.Save();
    }

    /// <summary>
    /// Adds a HiL program with the given name.
    /// </summary>
    public static void AddHilProgram(string name)
    {
        XmlBase.Project?.Element(XmlTags.HIL)?.Add(new XElement(name));
        XmlBase.Save();
    }
}