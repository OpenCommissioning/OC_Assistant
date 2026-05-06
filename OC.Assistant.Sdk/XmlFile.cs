using System.Xml.Linq;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Sdk;

/// <summary>
/// Represents a class that manages the project XML file, providing functionality to save and restore
/// plugins and project-related settings.
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
    /// Gets or sets the file path for the XML file.
    /// Changing this property triggers the reloading of the XML structure from the specified path.
    /// </summary>
    public string? Path
    {
        get;
        set
        {
            field = value;
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
            Logger.LogInfo(this, $"Project file {Path} has been loaded");
            return;
        }
        
        _doc = new XDocument(new XElement("Config"));
        
        Save();
        Reloaded?.Invoke();
        Logger.LogInfo(this, $"New project file {Path} has been created");
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
    /// Gets the Root <see cref="XElement"/>.
    /// </summary>
    public XElement? Root => _doc?.Root;
    
    /// <summary>
    /// Gets the Plugins <see cref="XElement"/>.
    /// </summary>
    public XElement Plugins => Root.GetOrCreateChild(nameof(Plugins));
    
    /// <summary>
    /// Gets the plugin elements as <see cref="XPlugin"/>.
    /// </summary>
    public IEnumerable<XPlugin> GetPluginElements() => Plugins.Elements().Select(x => new XPlugin(x));
}