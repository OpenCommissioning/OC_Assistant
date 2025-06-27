using EnvDTE;
using OC.Assistant.Core;
using OC.Assistant.Generator.EtherCat;
using OC.Assistant.Generator.Profinet;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Generators;

/// <summary>
/// Generator for HiL signals.
/// </summary>
internal static class Hil
{
    private const string FOLDER_NAME = nameof(Hil);

    /// <summary>
    /// Updates all HiL structures.
    /// </summary>
    public static void Update(DTE dte, ITcSmTreeItem plcProjectItem)
    {
        XmlFile.Instance.Hil.RemoveAll();
        XmlFile.Instance.Save();
        
        if (plcProjectItem.TryLookupChild(FOLDER_NAME) is not null)
        {
            plcProjectItem.DeleteChild(FOLDER_NAME);
        }
        
        new ProfinetGenerator(dte, FOLDER_NAME).Generate(plcProjectItem);
        new EtherCatGenerator(dte, FOLDER_NAME).Generate(plcProjectItem);
    }
}