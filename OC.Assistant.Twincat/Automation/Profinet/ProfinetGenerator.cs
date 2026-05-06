using System.Text;
using System.Xml.Linq;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat.Automation.Profinet;


internal class ProfinetGenerator(ITcSysManager15 tcSysManager, string folderName)
{
    public void Generate(ITcSmTreeItem plcProjectItem)
    {
        foreach (var item in tcSysManager.GetItem(TcShortcut.NODE_IO_DEVICES).GetChildren())
        {
            //Is not Profinet
            if (item.ItemSubType != (int) TcSmTreeItemSubType.ProfinetIoDevice) continue;
            
            //Is disabled 
            if (item.Disabled == DISABLED_STATE.SMDS_DISABLED) continue;

            //Export profinet to xti file
            var xtiPath = Path.Combine(AppData.Path, $"{item.Name}.xti");
            if (File.Exists(xtiPath)) File.Delete(xtiPath);
            item.Parent.ExportChild(item.Name, xtiPath);
            
            //Parse xti file and delete
            var profinetParser = new ProfinetParser(xtiPath, item.Name);
            File.Delete(xtiPath);
            
            GenerateFiles(plcProjectItem, item.Name, profinetParser.Variables, profinetParser.SafetyModules);
        }
    }
    
    private void GenerateFiles(ITcSmTreeItem plcProjectItem, string pnName, IEnumerable<ProfinetVariable> pnVars, IEnumerable<SafetyModule> safetyModules)
    {
        var inputs = new StringBuilder();
        var outputs = new StringBuilder();
        
        foreach (var pnVar in pnVars)
        {
            (pnVar.Direction == "Q" ? inputs : outputs).Append(pnVar.CreateDeclaration());
        }

        //Create safety program
        var safetyProgram = new SafetyProgram(safetyModules, pnName);

        //Create global variable list
        var hil = plcProjectItem.GetOrCreateChild(folderName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        var pnFolder = hil?.GetOrCreateChild(pnName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        
        //Create program
        if (pnFolder?.GetOrCreateChild($"PRG_{pnName}", TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG) is not { } prg) return;
        if (prg.GetOrCreateChild("InitRun", TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD) is not {} initRun) return;
        if (prg.CastTo<ITcPlcDeclaration>() is not {} prgDecl) return;
        if (prg.CastTo<ITcPlcImplementation>() is not {} prgImpl) return;
        if (initRun.CastTo<ITcPlcDeclaration>() is not {} initDecl) return;
        if (initRun.CastTo<ITcPlcImplementation>() is not {} initImpl) return;
        
        prgDecl.DeclarationText = 
            $"""
            PROGRAM {prg.Name}
            VAR
                bInitRun    : BOOL := TRUE;
                bReset      : BOOL;
                {safetyProgram.Declaration}
            END_VAR
            """;

        prgImpl.ImplementationText =
            $"""
             InitRun();
             {safetyProgram.Implementation}
             """;
        
        initDecl.DeclarationText = "METHOD PRIVATE InitRun";

        initImpl.ImplementationText =
            $"""
            IF NOT bInitRun THEN RETURN; END_IF
            bInitRun := FALSE;
            {safetyProgram.Parameter}
            """;
        
        pnFolder.CreateDutStruct($"{pnName}Inputs", inputs.ToString());
        pnFolder.CreateDutStruct($"{pnName}Outputs", outputs.ToString());
        pnFolder.CreateGvl(pnName, $"\tInputs : ST_{pnName}Inputs;\n\tOutputs : ST_{pnName}Outputs;\n");

        //Add program name to xml for project generator
        XmlFile.Instance.Hil.Add(new XElement("Program", $"PRG_{pnName}".MakePlcCompatible()));
        XmlFile.Instance.Save();
    }
}