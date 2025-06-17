using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace OC.Assistant.Core;

/// <summary>
/// Represents a class that manages the project XML file, providing functionality to save and restore
/// settings, plugins and project structure.
/// </summary>
public class XmlFile
{
    private static readonly Lazy<XmlFile> LazyInstance = new(() => new XmlFile());
    private XDocument? _doc;
    
    /// <summary>
    /// The private constructor.
    /// </summary>
    private XmlFile()
    {
    }
    
    /// <summary>
    /// Singleton instance of the <see cref="XmlFile"/>.
    /// </summary>
    public static XmlFile Instance => LazyInstance.Value;

    /// <summary>
    /// Gets the path of the XML file. Can be null of not connected.
    /// </summary>
    public string? Path { get; private set; }

    /// <summary>
    /// Sets the directory of the XML file.<br/>
    /// The <see cref="Path"/> is set to the directory combined with the file name.
    /// <param name="path">The path of the project folder.</param>
    /// </summary>
    public void SetDirectory(string path)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        Path = System.IO.Path.Combine(path, $"{assemblyName}.xml");
        Reload();
    }

    /// <summary>
    /// Is raised when the XML file has been reloaded.
    /// </summary>
    public event Action? Reloaded;
    
    /// <summary>
    /// Reloads or creates the XML file.
    /// </summary>
    public void Reload()
    {
        if (Path is null) return;
        if (File.Exists(Path))
        {
            _doc = XDocument.Load(Path);
            Reloaded?.Invoke();
            return;
        }
        
        _doc = new XDocument(
            new XElement(XmlTags.ROOT,
                new XElement(XmlTags.SETTINGS,
                    new XElement(XmlTags.PLC_PROJECT_NAME, "OC"),
                    new XElement(XmlTags.PLC_TASK_NAME, "PlcTask"),
                    new XElement(XmlTags.TASK_AUTO_UPDATE)),
                new XElement(XmlTags.PLUGINS),
                new XElement(XmlTags.PROJECT,
                    new XElement(XmlTags.HIL),
                    new XElement(XmlTags.MAIN))));
        
        _doc.Save(Path);
        Reloaded?.Invoke();
    }

    /// <summary>
    /// Saves the current configuration.
    /// </summary>
    public void Save()
    {
        if (Path is null) return;
        _doc?.Save(Path);
    }
    
    /// <summary>
    /// Returns the <see cref="XmlTags.SETTINGS"/> <see cref="XElement"/>.
    /// </summary>
    public XElement Settings => GetOrCreateChild(_doc?.Root, XmlTags.SETTINGS);
    
    /// <summary>
    /// Returns the <see cref="XmlTags.PLUGINS"/> <see cref="XElement"/>.
    /// </summary>
    public XElement Plugins => GetOrCreateChild(_doc?.Root, XmlTags.PLUGINS);
    
    /// <summary>
    /// Returns the <see cref="XmlTags.PROJECT"/> <see cref="XElement"/>.
    /// </summary>
    public XElement Project => GetOrCreateChild(_doc?.Root, XmlTags.PROJECT);
    
    /// <summary>
    /// Gets the main program element.
    /// </summary>
    public XElement ProjectMain => GetOrCreateChild(Project, XmlTags.MAIN);
    
    /// <summary>
    /// Gets the HiL element.
    /// </summary>
    public XElement ProjectHil => GetOrCreateChild(Project, XmlTags.HIL);
    
    /// <summary>
    /// Gets the plugin elements.
    /// </summary>
    public IEnumerable<XElement> PluginElements => Plugins.Elements(XmlTags.PLUGIN);

    /// <summary>
    /// Tries to get a child from a given parent <see cref="XElement"/>.<br/>
    /// Creates the child if it doesn't exist. 
    /// </summary>
    /// <param name="parent">The parent <see cref="XElement"/>.</param>
    /// <param name="childName">The name of the child <see cref="XElement"/> to get or create.</param>
    /// <returns>The child <see cref="XElement"/> with the given name.</returns>
    public static XElement GetOrCreateChild(XElement? parent, string childName)
    {
        var element = parent?.Element(childName);
        if (element is not null) return element;
        element = new XElement(childName);
        parent?.Add(element);
        return element;
    }
    
    /// <summary>
    /// Gets or sets the PlcProjectName value.
    /// </summary>
    public string PlcProjectName
    {
        get => GetOrCreateChild(Settings, XmlTags.PLC_PROJECT_NAME).Value;
        set
        {
            var element = GetOrCreateChild(Settings, XmlTags.PLC_PROJECT_NAME);
            element.Value = value;
            Save();
        }
    }
    
    /// <summary>
    /// Gets or sets the PlcTaskName value.
    /// </summary>
    public string PlcTaskName
    {
        get => GetOrCreateChild(Settings, XmlTags.PLC_TASK_NAME).Value;
        set
        {
            var element = GetOrCreateChild(Settings, XmlTags.PLC_TASK_NAME);
            element.Value = value;
            Save();
        }
    }
    
    /// <summary>
    /// Implements a new client configuration.
    /// </summary>
    public void ClientUpdate(XElement config)
    {
        try
        {
            ProjectMain.ReplaceNodes(config.Elements());
            Save();
        }
        catch (Exception e)
        {
            Sdk.Logger.LogWarning(nameof(XmlFile), e.Message);
        }
    }

    /// <summary>
    /// Removes all HiL programs.
    /// </summary>
    public void ClearHilPrograms()
    {
        ProjectHil.RemoveAll();
        Save();
    }

    /// <summary>
    /// Adds a HiL program with the given name.
    /// </summary>
    public void AddHilProgram(string name)
    {
        ProjectHil.Add(new XElement("Program", $"PRG_{name}".MakePlcCompatible()));
        Save();
    }
}