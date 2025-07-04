using System.IO;
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
    private string? _path;
    
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
    /// Gets or sets the file path for the XML file.
    /// Changing this property triggers the reloading of the XML structure from the specified path.
    /// </summary>
    public string? Path
    {
        get => _path;
        set
        {
            _path = value;
            Reload();
        }
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
            new XElement("Config",
                new XElement(nameof(Settings),
                    new XElement(nameof(PlcProjectName), "OC"),
                    new XElement(nameof(PlcTaskName), "PlcTask")),
                new XElement(nameof(Plugins)),
                new XElement(nameof(Project),
                    new XElement(nameof(Hil)),
                    new XElement(nameof(Main)))));
        
        Save();
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
    /// Gets the Settings <see cref="XElement"/>.
    /// </summary>
    public XElement Settings => (_doc?.Root).GetOrCreateChild(nameof(Settings));
    
    /// <summary>
    /// Gets the Plugins <see cref="XElement"/>.
    /// </summary>
    public XElement Plugins => (_doc?.Root).GetOrCreateChild(nameof(Plugins));
    
    /// <summary>
    /// Gets the Project <see cref="XElement"/>.
    /// </summary>
    public XElement Project => (_doc?.Root).GetOrCreateChild(nameof(Project));
    
    /// <summary>
    /// Gets the HiL element.
    /// </summary>
    public XElement Hil => Project.GetOrCreateChild(nameof(Hil));
    
    /// <summary>
    /// Gets the main program <see cref="XElement"/> or sets its content.
    /// </summary>
    public XElement Main
    {
        get => Project.GetOrCreateChild(nameof(Main));
        set
        {
            Project.GetOrCreateChild(nameof(Main)).ReplaceNodes(value.Nodes());
            Save();       
        }
    }

    /// <summary>
    /// Gets or sets the PlcProjectName value.
    /// </summary>
    public string PlcProjectName
    {
        get => Settings.GetOrCreateChild(nameof(PlcProjectName)).Value;
        set
        {
            Settings.GetOrCreateChild(nameof(PlcProjectName)).Value = value;
            Save();
        }
    }
    
    /// <summary>
    /// Gets or sets the PlcTaskName value.
    /// </summary>
    public string PlcTaskName
    {
        get => Settings.GetOrCreateChild(nameof(PlcTaskName)).Value;
        set
        {
            Settings.GetOrCreateChild(nameof(PlcTaskName)).Value = value;
            Save();
        }
    }
}