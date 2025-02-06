using System.IO;
using System.Xml.Linq;

namespace OC.Assistant.Core;

public class XmlFile
{
    private const string DEFAULT_FILE_NAME = "OC.Assistant.xml";
    private static readonly Lazy<XmlFile> LazyInstance = new(() => new XmlFile());
    private static string? _path;
    private XDocument? _doc;

    /// <summary>
    /// Gets or sets the path of the xml file.
    /// Must be set before using the <see cref="Instance"/>.
    /// Can also be set by setting the <see cref="Directory"/> property.
    /// </summary>
    public static string? Path
    {
        get => _path;
        set
        {
            if (LazyInstance.IsValueCreated) return;
            _path = value;
        }
    }
    
    /// <summary>
    /// Sets the directory of the xml file.<br/>
    /// The <see cref="Path"/> is set to the directory combined with the <see cref="DEFAULT_FILE_NAME"/>.
    /// </summary>
    public static string? Directory
    {
        set
        {
            if (LazyInstance.IsValueCreated || value is null) return;
            _path = System.IO.Path.Combine(value, DEFAULT_FILE_NAME);
        }
    }
    
    /// <summary>
    /// Singleton instance of the <see cref="XmlFile"/>.
    /// </summary>
    public static XmlFile Instance => LazyInstance.Value;

    /// <summary>
    /// Is raised when the xml file has been reloaded.
    /// </summary>
    public event Action? Reloaded;
    
    /// <summary>
    /// The private constructor. Creates a new xml file if not exists.
    /// </summary>
    private XmlFile()
    {
        if (Path is null)
        {
            throw new NullReferenceException($"{nameof(Path)} must be set before accessing the singleton instance.");
        }
        
        if (LazyInstance.IsValueCreated) return;
        
        if (File.Exists(Path))
        {
            _doc = XDocument.Load(Path);
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
    }
    
    /// <summary>
    /// Reloads the xml file.
    /// </summary>
    public void Reload()
    {
        if (Path is null) return;
        _doc = XDocument.Load(Path);
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
    public XElement? Settings => _doc?.Root?.Element(XmlTags.SETTINGS);
    
    /// <summary>
    /// Returns the <see cref="XmlTags.PLUGINS"/> <see cref="XElement"/>.
    /// </summary>
    public XElement? Plugins => _doc?.Root?.Element(XmlTags.PLUGINS);
    
    /// <summary>
    /// Returns the <see cref="XmlTags.PROJECT"/> <see cref="XElement"/>.
    /// </summary>
    public XElement? Project => _doc?.Root?.Element(XmlTags.PROJECT);

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
        get => Settings?.Element(XmlTags.PLC_PROJECT_NAME)?.Value ?? "";
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
        get => Settings?.Element(XmlTags.PLC_TASK_NAME)?.Value ?? "";
        set
        {
            var element = GetOrCreateChild(Settings, XmlTags.PLC_TASK_NAME);
            element.Value = value;
            Save();
        }
    }
}