using System.Text.Json;
using OC.Assistant.Sdk;

namespace OC.Assistant;

/// <summary>
/// Represents the settings structure.
/// </summary>
public class AppSettings
{
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
    public int PosX { get; set; }
    /// <summary>
    /// Gets or sets the Y position of the application window.
    /// </summary>
    public int PosY { get; set; }
    /// <summary>
    /// Gets or sets the console height within the application window.
    /// </summary>
    public int ConsoleHeight { get; set; } = 140;
    /// <summary>
    /// Gets or sets the IP address of the WebApiServer.
    /// </summary>
    public string IpAddress { get; set; } = "127.0.0.1";
    /// <summary>
    /// Gets or sets the port of the WebApiServer.
    /// </summary>
    public int WebApiPort { get; set; } = 50110;
    /// <summary>
    /// Gets or sets the port of the WebApiServer.
    /// </summary>
    public int PluginServerPort { get; set; } = 50100;
}

/// <summary>
/// <see cref="AppSettings"/> extension methods.
/// </summary>
public static class AppSettingsExtension
{
    private static string Path => $"{AppData.Path}\\settings.json";
    private static JsonSerializerOptions SerializerOptions => new() { WriteIndented = true };
    
    extension(AppSettings settings)
    {
        /// <summary>
        /// Reads and deserializes the <see cref="AppSettings"/> from a specific path.
        /// </summary>
        public AppSettings Read()
        {
            try
            {
                settings = JsonSerializer
                               .Deserialize<AppSettings>(System.IO.File.ReadAllText(Path)) ?? 
                           settings;
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(AppSettings), e.Message);
            }
        
            return settings;
        }

        /// <summary>
        /// Serializes and writes the <see cref="AppSettings"/> to a specific path.
        /// </summary>
        public void Write()
        {
            try
            {
                var contents = JsonSerializer.Serialize(settings, SerializerOptions);
                System.IO.File.WriteAllText(Path, contents);
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(AppSettings), e.Message);
            }
        }
    }
}