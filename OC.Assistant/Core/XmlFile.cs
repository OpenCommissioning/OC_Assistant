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
    /// The name of the root node.
    /// </summary>
    private const string ROOT = "Config";

    /// <summary>
    /// The node name of the plugin category.
    /// </summary>
    private const string PLUGINS = "Plugins";
    
    /// <summary>
    /// The node name of the plc project category.
    /// </summary>
    private const string PROJECT = "Project";

    /// <summary>
    /// The node name of the general settings.
    /// </summary>
    private const string SETTINGS = "Settings";
    
    /// <summary>
    /// The node name of the projectName setting.
    /// </summary>
    private const string PLC_PROJECT_NAME = "PlcProjectName";
    
    /// <summary>
    /// The node name of the taskName setting.
    /// </summary>
    private const string PLC_TASK_NAME = "PlcTaskName";
    
    /// <summary>
    /// The node name of Hil.
    /// </summary>
    private const string HIL = "Hil";
    
    /// <summary>
    /// The node name of the plc project root.
    /// </summary>
    private const string MAIN = "Main";
    
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
            new XElement(ROOT,
                new XElement(SETTINGS,
                    new XElement(PLC_PROJECT_NAME, "OC"),
                    new XElement(PLC_TASK_NAME, "PlcTask")),
                new XElement(PLUGINS),
                new XElement(PROJECT,
                    new XElement(HIL),
                    new XElement(MAIN))));
        
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
    /// Gets the <see cref="SETTINGS"/> <see cref="XElement"/>.
    /// </summary>
    public XElement Settings => (_doc?.Root).GetOrCreateChild(SETTINGS);
    
    /// <summary>
    /// Gets the <see cref="PLUGINS"/> <see cref="XElement"/>.
    /// </summary>
    public XElement Plugins => (_doc?.Root).GetOrCreateChild(PLUGINS);
    
    /// <summary>
    /// Gets the <see cref="PROJECT"/> <see cref="XElement"/>.
    /// </summary>
    public XElement Project => (_doc?.Root).GetOrCreateChild(PROJECT);
    
    /// <summary>
    /// Gets the main program element.
    /// </summary>
    public XElement ProjectMain => Project.GetOrCreateChild(MAIN);
    
    /// <summary>
    /// Gets the HiL element.
    /// </summary>
    public XElement ProjectHil => Project.GetOrCreateChild(HIL);
    
    /// <summary>
    /// Gets the plugin elements as <see cref="XPlugin"/>.
    /// </summary>
    public IEnumerable<XPlugin> PluginElements => Plugins.Elements().Select(x => new XPlugin(x));

    /// <summary>
    /// Retrieves the <see cref="XPlugin"/> by the given name.
    /// </summary>
    /// <param name="name">The name of the plugin to retrieve.</param>
    /// <returns>The first <see cref="XPlugin"/> corresponding to the name, if any.</returns>
    public XPlugin? GetPlugin(string name) => PluginElements.FirstOrDefault(x => x.Name == name);

    /// <summary>
    /// Removes all <see cref="XPlugin"/> elements by the given name.
    /// </summary>
    /// <param name="name">The name of the plugin to remove.</param>
    public void RemovePlugin(string name)
    {
        foreach (var xPlugin in PluginElements.Where(x => x.Name == name))
        {
            xPlugin.Element.Remove();
        }
        
        Save();
    }
    
    /// <summary>
    /// Gets or sets the PlcProjectName value.
    /// </summary>
    public string PlcProjectName
    {
        get => Settings.GetOrCreateChild(PLC_PROJECT_NAME).Value;
        set
        {
            var element = Settings.GetOrCreateChild(PLC_PROJECT_NAME);
            element.Value = value;
            Save();
        }
    }
    
    /// <summary>
    /// Gets or sets the PlcTaskName value.
    /// </summary>
    public string PlcTaskName
    {
        get => Settings.GetOrCreateChild(PLC_TASK_NAME).Value;
        set
        {
            var element = Settings.GetOrCreateChild(PLC_TASK_NAME);
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