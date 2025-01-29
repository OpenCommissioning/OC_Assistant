using System.Diagnostics.CodeAnalysis;
using System.IO;
using OC.Assistant.Core;
using OC.Assistant.Core.TwinCat;
using TCatSysManagerLib;

namespace OC.Assistant.Generator.Profinet;

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
internal class ProfinetGenerator(IProjectConnector projectConnector, string folderName)
{
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
                
                GenerateFiles(plcProjectItem, item.Name, profinetParser.Variables, profinetParser.SafetyModules);
            }
        });
    }
    
    private void GenerateFiles(ITcSmTreeItem plcProjectItem, string pnName, IEnumerable<ProfinetVariable> pnVars, IEnumerable<SafetyModule> safetyModules)
    {
        //Declaration variables
        const string declarationTemplate = "{attribute 'TcLinkTo' := '$LINK$'}\n$VARNAME$ AT %$DIRECTION$* : $VARTYPE$;\n";
        var gvlVariables = pnVars.Aggregate("", (current, var) => current + declarationTemplate
            .Replace(Tags.VAR_TYPE, var.Type)
            .Replace(Tags.DIRECTION, var.Direction)
            .Replace(Tags.LINK, var.Link)
            .Replace(Tags.VAR_NAME, var.Name));

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
            PROGRAM PRG_{pnName}
            VAR
                bInitRun    : BOOL := TRUE;
                bReset      : BOOL;
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

        pnFolder.CreateGvl(pnName, gvlVariables + safetyProgram.Declaration);

        //Add program name to xml for project generator
        XmlFile.AddHilProgram(pnName);
    }
}