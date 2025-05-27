using System.Diagnostics.CodeAnalysis;
using System.IO;
using EnvDTE;
using OC.Assistant.Core;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Profinet;

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
internal class ProfinetGenerator(DTE dte, string folderName)
{
    public void Generate(ITcSmTreeItem plcProjectItem)
    {
        var tcSysManager = dte.GetTcSysManager();
        if (tcSysManager is null) return;
        
        if (!tcSysManager.TryLookupTreeItem(TcShortcut.IO_DEVICE, out var ioItem))
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
            
            GenerateFiles(plcProjectItem, item.Name, profinetParser.Variables, profinetParser.SafetyModules);
        }
    }
    
    private void GenerateFiles(ITcSmTreeItem plcProjectItem, string pnName, IEnumerable<ProfinetVariable> pnVars, IEnumerable<SafetyModule> safetyModules)
    {
        var gvlVariables = "";

        foreach (var pnVar in pnVars)
        {
            gvlVariables += pnVar.CreateGvlDeclaration();
        }

        //Create safety program
        var safetyProgram = new SafetyProgram(safetyModules, pnName);

        //Create global variable list
        var hil = plcProjectItem.GetOrCreateChild(folderName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        var pnFolder = hil?.GetOrCreateChild(pnName, TREEITEMTYPES.TREEITEMTYPE_PLCFOLDER);
        
        //Create program
        if (pnFolder?.GetOrCreateChild($"PRG_{pnName}", TREEITEMTYPES.TREEITEMTYPE_PLCPOUPROG) is not { } prg) return;
        if (prg.GetOrCreateChild("InitRun", TREEITEMTYPES.TREEITEMTYPE_PLCMETHOD) is not {} initRun) return;
        if (prg is not ITcPlcDeclaration prgDecl) return;
        if (prg is not ITcPlcImplementation prgImpl) return;
        if (initRun is not ITcPlcDeclaration initDecl) return;
        if (initRun is not ITcPlcImplementation initImpl) return;
        
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

        pnFolder.CreateGvl(pnName, gvlVariables);

        //Add program name to xml for project generator
        XmlFile.AddHilProgram(pnName);
    }
}