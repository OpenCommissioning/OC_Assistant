using OC.Assistant.Sdk;
using OC.Assistant.Twincat.Automation.EtherCat;
using OC.Assistant.Twincat.Automation.Profinet;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat.Automation;

/// <summary>
/// Generator for HiL signals.
/// </summary>
internal static class HilGenerator
{
    private const string FOLDER_NAME = "Hil";

    /// <summary>
    /// Updates all HiL structures.
    /// </summary>
    public static void Update(ITcSysManager15 tcSysManager, ITcSmTreeItem plcProjectItem)
    {
        XmlFile.Instance.Hil.RemoveAll();
        XmlFile.Instance.Save();
        
        if (plcProjectItem.GetChild(FOLDER_NAME) is not null)
        {
            plcProjectItem.DeleteChild(FOLDER_NAME);
        }
        
        new ProfinetGenerator(tcSysManager, FOLDER_NAME).Generate(plcProjectItem);
        new EtherCatGenerator(tcSysManager, FOLDER_NAME).Generate(plcProjectItem);
    }
}