using System.Text.Json;
using OC.Assistant.Sdk;

namespace OC.Assistant.Models;

/// <summary>
/// Represents the settings structure.
/// </summary>
public class AppSettings
{
    private static string Path => System.IO.Path.Combine(AppData.Path, "settings.json");
    private static JsonSerializerOptions SerializerOptions => new() { WriteIndented = true };
    
    /// <summary>
    /// Gets or sets the height of the application window.
    /// </summary>
    public int Height { get; set; } = 900;
    /// <summary>
    /// Gets or sets the width of the application window.
    /// </summary>
    public int Width { get; set; } = 1280;
    /// <summary>
    /// Gets or sets the X position of the application window.
    /// </summary>
    public int Left { get; set; }
    /// <summary>
    /// Gets or sets the Y position of the application window.
    /// </summary>
    public int Top { get; set; }
    /// <summary>
    /// Gets or sets the IP address of the WebApiServer.
    /// </summary>
    public string IpAddress { get; set; } = "127.0.0.1";
    /// <summary>
    /// Gets or sets the port of the WebApiServer.
    /// </summary>
    public int WebApiPort { get; set; } = 50110;
    /// <summary>
    /// Gets or sets a value whether the app implements the PluginServer.
    /// </summary>
    public bool EnablePluginServer { get; set; }
    
    /// <summary>
    /// Reads and deserializes the <see cref="AppSettings"/> from a specific path.
    /// </summary>
    public AppSettings Read()
    {
        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(Path));
            if (settings is null) throw new Exception($"Could not read settings from {Path}");
            this.CopyPropertyValues(settings);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
        
        return this;
    }

    /// <summary>
    /// Serializes and writes the <see cref="AppSettings"/> to a specific path.
    /// </summary>
    public void Write()
    {
        try
        {
            var contents = JsonSerializer.Serialize(this, SerializerOptions);
            File.WriteAllText(Path, contents);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
}