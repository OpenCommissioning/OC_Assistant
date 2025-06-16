using System.Xml.Linq;
using OC.Assistant.Core;

namespace OC.Assistant.Generator;

/// <summary>
/// <see cref="Core.XmlFile"/> extension to write and read HiL configurations.
/// </summary>
internal static class XmlFile
{
    private static Core.XmlFile XmlBase => Core.XmlFile.Instance;

    /// <summary>
    /// Gets the main program element.
    /// </summary>
    public static XElement Main => Core.XmlFile.GetOrCreateChild(XmlBase.Project, XmlTags.MAIN);
    
    /// <summary>
    /// Gets the HiL element.
    /// </summary>
    public static XElement HiL => Core.XmlFile.GetOrCreateChild(XmlBase.Project, XmlTags.HIL);
    
    /// <summary>
    /// Gets the plugin elements.
    /// </summary>
    public static IEnumerable<XElement> PluginElements => XmlBase.Plugins.Elements(XmlTags.PLUGIN);
    
    /// <summary>
    /// Implements a new client configuration.
    /// </summary>
    public static void ClientUpdate(XElement config)
    {
        try
        {
            Main.ReplaceNodes(config.Elements());
            XmlBase.Save();
        }
        catch (Exception e)
        {
            Sdk.Logger.LogWarning(nameof(XmlFile), e.Message);
        }
    }

    /// <summary>
    /// Removes all HiL programs.
    /// </summary>
    public static void ClearHilPrograms()
    {
        HiL.RemoveAll();
        XmlBase.Save();
    }

    /// <summary>
    /// Adds a HiL program with the given name.
    /// </summary>
    public static void AddHilProgram(string name)
    {
        HiL.Add(new XElement("Program", $"PRG_{name}".MakePlcCompatible()));
        XmlBase.Save();
    }
}