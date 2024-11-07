using System.IO;
using System.Text;
using OC.Assistant.Core;

namespace OC.Assistant.Generator;

/// <summary>
/// Extension methods to create strings based on resource templates.
/// </summary>
internal static class ResourceStringExtension
{
    /// <summary>
    /// Reads the assembly resource by the given name.
    /// </summary>
    /// <param name="resourceName">The name of the resource including the file extension.</param>
    /// <returns>The resource stream as string.</returns>
    private static string ResourceToString(string resourceName)
    {
        var assembly = typeof(ResourceStringExtension).Assembly;
        var name = assembly.GetName().Name;
        var resource = assembly.GetManifestResourceStream($"{name}.Resources.{resourceName}");
        if (resource is null) return "";
        using var reader = new StreamReader(resource, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    
    /// <summary>
    /// Creates a structure based on the TcDUT.xml resource template.
    /// </summary>
    /// <param name="variables">The concatenated string with the structure variables.</param>
    /// <param name="folderName">The name of the folder where the structure will be located.</param>
    /// <param name="baseName">The parent name of the structure.</param>
    /// <param name="uniqueName">The unique name of the structure.</param>
    /// <returns>The name of the DUT.</returns>
    public static string CreateDut(this string variables, string folderName, string baseName, string uniqueName)
    {
        var dut = ResourceToString("TcDUT.xml")
            .Replace(Tags.NAME, $"{baseName}{uniqueName}")
            .Replace(Tags.GUID, Guid.NewGuid().ToString("B"))
            .Replace(Tags.DECLARATION, variables);
        var dutName = $"ST_{baseName}{uniqueName}";
        File.WriteAllText($@"{AppData.Path}\{folderName}\{baseName}\{dutName}.TcDUT", dut);
        return dutName;
    }

    /// <summary>
    /// Generates a global variable list based on the TcGVL template.
    /// </summary>
    /// <param name="variables">The concatenated string with the GVL variables.</param>
    /// <param name="folderName">The name of the folder where the GVL will be located.</param>
    /// <param name="name">The name of the GVL.</param>
    public static void CreateGvl(this string variables, string folderName, string name)
    {
        var gvl = ResourceToString("TcGVL.xml")
            .Replace(Tags.NAME, name)
            .Replace(Tags.GUID, Guid.NewGuid().ToString("B"))
            .Replace(Tags.DECLARATION, variables);
        File.WriteAllText($@"{AppData.Path}\{folderName}\{name}\GVL_{name}.TcGVL", gvl);
    }
        
    /// <summary>
    /// Generates a program based on the TcPOU template.
    /// </summary>
    /// <param name="program">The concatenated implementation text.</param>
    /// <param name="folderName">The name of the folder where the program will be located.</param>
    /// <param name="name">The name of the program.</param>
    /// <param name="initRun">The concatenated implementation text for the InitRun method.</param>
    public static void CreatePou(this string program, string folderName, string name, string initRun)
    {
        var pou = ResourceToString("TcPOU.xml")
            .Replace(Tags.NAME, name)
            .Replace(Tags.GUID + "0", Guid.NewGuid().ToString("B"))
            .Replace(Tags.GUID + "1", Guid.NewGuid().ToString("B"))
            .Replace(Tags.IMPLEMENTATION + "0", program)
            .Replace(Tags.IMPLEMENTATION + "1", initRun);
        File.WriteAllText($@"{AppData.Path}\{folderName}\{name}\PRG_{name}.TcPOU", pou);
    }
}