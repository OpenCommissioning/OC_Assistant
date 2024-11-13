using System.IO;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Profinet;

internal class ProfinetGenerator(IProjectConnector projectConnector, string folderName)
{
    private const string VAR_DECLARATION = "{attribute 'TcLinkTo' := '$LINK$'}\n$VARNAME$ AT %$DIRECTION$* : $VARTYPE$;\n";

    public void Generate(ITcSmTreeItem plcProjectItem)
    {
        Retry.Invoke(() =>
        {
            if (projectConnector.TcSysManager is null) return;
            
            if (!projectConnector.TcSysManager.TryLookupTreeItem(TcShortcut.IO_DEVICE, out var ioItem))
            {
                return;
            }
            
            foreach (ITcSmTreeItem item in ioItem)
            {
                //Is not Profinet
                if (item.ItemSubType != (int) TcSmTreeItemSubType.ProfinetIoDevice) continue;
                
                //Is disabled 
                if (item.Disabled == DISABLED_STATE.SMDS_DISABLED) continue;

                //Export profinet to xti file
                var xtiPath = $"{AppData.Path}\\{item.Name}.xti";
                if (File.Exists(xtiPath)) File.Delete(xtiPath);
                item.Parent.ExportChild(item.Name, xtiPath);
                
                //Parse xti file and delete
                var profinetParser = new ProfinetParser(xtiPath, item.Name);
                File.Delete(xtiPath);
                
                GenerateFiles(item.Name, profinetParser.Variables, profinetParser.SafetyModules);
            }
        });

        
        plcProjectItem.TcIntegrate(folderName);
    }
    
    private void GenerateFiles(string pnName, IEnumerable<ProfinetVariable> pnVars, IEnumerable<SafetyModule> safetyModules)
    {
        //Declaration variables
        var gvl = pnVars.Aggregate("", (current, var) => current + VAR_DECLARATION
            .Replace(Tags.VAR_TYPE, var.Type)
            .Replace(Tags.DIRECTION, var.Direction)
            .Replace(Tags.LINK, var.Link)
            .Replace(Tags.VAR_NAME, var.Name));

        //Create safety program
        var safetyProgram = new SafetyProgram(safetyModules, pnName);

        //Create or get folder
        Directory.CreateDirectory($"{AppData.Path}\\{folderName}\\{pnName}");

        //Create global variable list
        (gvl + safetyProgram.Declaration).CreateGvl(folderName, pnName);
            
        //Create program
        safetyProgram.Implementation.CreatePou(folderName, pnName, safetyProgram.Parameter);

        //Add program name to xml for project generator
        XmlFile.AddHilProgram(pnName);
    }
}