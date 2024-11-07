using System.IO;

namespace OC.Assistant.Core;

public static class AppData
{
    private static readonly string PathPreset = 
        $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\OC.Assistant";

    /// <summary>
    /// Gets the path of the user specific directory.
    /// </summary>
    public static string Path
    {
        get
        {
            Directory.CreateDirectory(PathPreset);
            return PathPreset;
        }
    }
    
    /// <summary>
    /// Gets the path of the global settings for the application.
    /// </summary>
    public static string SettingsFilePath => $"{Path}\\settings.json";

    /// <summary>
    /// Gets the path of the log file.
    /// </summary>
    public static string LogFilePath => $"{Path}\\log.txt";

    /// <summary>
    /// Gets the path for the file to store temporary project information.
    /// </summary>
    public static string PreselectedProject => System.IO.Path.Combine(Path, "projectPath.tmp");
}