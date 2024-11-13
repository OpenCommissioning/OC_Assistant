using System.Text.Json;
using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant;

/// <summary>
/// Represents the settings structure.
/// </summary>
public class Settings
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
}

/// <summary>
/// <see cref="Settings"/> extension methods.
/// </summary>
public static class SettingsExtension
{
    /// <summary>
    /// Reads and deserializes the <see cref="Settings"/> from a specific path.
    /// </summary>
    public static Settings Read(this Settings settings)
    {
        try
        {
            settings = JsonSerializer
                .Deserialize<Settings>(System.IO.File.ReadAllText(AppData.SettingsFilePath)) ?? 
                       settings;
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(Settings), e.Message);
        }
        
        return settings;
    }

    /// <summary>
    /// Serializes and writes the <see cref="Settings"/> to a specific path.
    /// </summary>
    public static void Write(this Settings settings)
    {
        try
        {
            var contents = JsonSerializer.Serialize(settings);
            System.IO.File.WriteAllText(AppData.SettingsFilePath, contents);
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(Settings), e.Message);
        }
    }
}