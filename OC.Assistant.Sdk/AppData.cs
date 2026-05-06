namespace OC.Assistant.Sdk;

/// <summary>
/// ApplicationData information.
/// </summary>
public static class AppData
{
    /// <summary>
    /// Gets the path of the user-specific directory.
    /// </summary>
    public static string Path
    {
        get
        {
            Directory.CreateDirectory(field);
            return field;
        }
    } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OC.Assistant") ;
}