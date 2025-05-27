using OC.Assistant.Core;

namespace OC.Assistant.Generator.Profinet;

internal class SafetyProgram
{
    public string Declaration { get; } = "";
    public string Implementation { get; } = "";
    public string Parameter { get; } = "";
    
    public SafetyProgram(IEnumerable<SafetyModule> modules, string pnName)
    {
        const string info = $"//Profisafe Device {Tags.DEVICE} - Slot {Tags.SLOT} - SubSlot {Tags.SUB_SLOT}\n";
        const string fbDecl = $"\n\t{Tags.VAR_NAME}: FB_ProfisafeDevice; {info}";
        const string configPort = $"{Tags.INSTANCE}.stConfig.nPort := {Tags.VALUE};\n";
        const string configSlot = $"{Tags.INSTANCE}.stConfig.nSlot := {Tags.VALUE};\n";
        const string configSubSlot = $"{Tags.INSTANCE}.stConfig.nSubSlot := {Tags.VALUE};\n";
        const string configHstSize = $"{Tags.INSTANCE}.stConfig.nHstDatasize := {Tags.VALUE};\n";
        const string configDevSize = $"{Tags.INSTANCE}.stConfig.nDevDatasize := {Tags.VALUE};\n";
        const string configInputAddr = $"{Tags.INSTANCE}.stConfig.pControlData := ADR({Tags.VAR_NAME});\n";
        const string configOutputAddr = $"{Tags.INSTANCE}.stConfig.pStatusData := ADR({Tags.VAR_NAME});\n";
        const string configDevDataAddr = $"{Tags.INSTANCE}.stConfig.pDevData := ADR({Tags.NAME}.{Tags.VAR_NAME});\n";
        const string configHstDataAddr = $"{Tags.INSTANCE}.stConfig.pHstData := ADR({Tags.NAME}.{Tags.VAR_NAME});\n";
        const string call = $"{Tags.INSTANCE}(bReset := bReset);\n";
        
        foreach (var module in modules.Where(module => module.PnName == pnName && module.IsValid))
        {
            var hstVar = module.HstVariable;
            var devVar = module.DevVariable;
            if (hstVar is null || devVar is null) continue;
            
            Declaration += fbDecl
                .Replace(Tags.VAR_NAME, module.Name)
                .Replace(Tags.DEVICE, module.BoxName)
                .Replace(Tags.SLOT, module.Slot.ToString())
                .Replace(Tags.SUB_SLOT, module.SubSlot.ToString());
            
            Declaration += hstVar.CreatePrgDeclaration();
            Declaration += devVar.CreatePrgDeclaration();
            
            Implementation += call
                .Replace(Tags.INSTANCE, module.Name);
            
            Parameter += $"\n{info}"
                .Replace(Tags.DEVICE, module.BoxName)
                .Replace(Tags.SLOT, module.Slot.ToString())
                .Replace(Tags.SUB_SLOT, module.SubSlot.ToString());
            Parameter += configPort
                .Replace(Tags.VALUE, module.Port.ToString());
            Parameter += configSlot
                .Replace(Tags.VALUE, module.Slot.ToString());
            Parameter += configSubSlot
                .Replace(Tags.VALUE, module.SubSlot.ToString());
            Parameter += configHstSize
                .Replace(Tags.VALUE, module.HstSize.ToString());
            Parameter += configDevSize
                .Replace(Tags.VALUE, module.DevSize.ToString());
            Parameter += configInputAddr
                .Replace(Tags.VAR_NAME, module.HstDataName);
            Parameter += configOutputAddr
                .Replace(Tags.VAR_NAME, module.DevDataName);
            Parameter += configHstDataAddr
                .Replace(Tags.VAR_NAME, module.HstDataName);
            Parameter += configDevDataAddr
                .Replace(Tags.VAR_NAME, module.DevDataName);
            Parameter = Parameter
                .Replace(Tags.NAME, $"GVL_{pnName}".MakePlcCompatible())
                .Replace(Tags.INSTANCE, module.Name);
        }
    }
}