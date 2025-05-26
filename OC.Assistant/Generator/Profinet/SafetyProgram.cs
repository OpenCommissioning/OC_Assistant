using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator.Profinet;

internal class SafetyProgram
{
    private const string INFO = "//Profisafe Device $DEVICE$ - Slot $SLOT$ - SubSlot $SUBSLOT$\n";
    private const string FB_DECL = $"\n\t$VARNAME$: FB_ProfisafeDevice; {INFO}";
    private const string CONFIG_PORT = "$INSTANCE$.stConfig.nPort := $VALUE$;\n";
    private const string CONFIG_SLOT = "$INSTANCE$.stConfig.nSlot := $VALUE$;\n";
    private const string CONFIG_SUB_SLOT = "$INSTANCE$.stConfig.nSubSlot := $VALUE$;\n";
    private const string CONFIG_HST_SIZE = "$INSTANCE$.stConfig.nHstDatasize := $VALUE$;\n";
    private const string CONFIG_DEV_SIZE = "$INSTANCE$.stConfig.nDevDatasize := $VALUE$;\n";
    private const string CONFIG_INPUT_ADDR = "$INSTANCE$.stConfig.pControlData := ADR($VARNAME$);\n";
    private const string CONFIG_OUTPUT_ADDR = "$INSTANCE$.stConfig.pStatusData := ADR($VARNAME$);\n";
    private const string CONFIG_DEV_DATA_ADDR = "$INSTANCE$.stConfig.pDevData := ADR($NAME$.$VARNAME$);\n";
    private const string CONFIG_HST_DATA_ADDR = "$INSTANCE$.stConfig.pHstData := ADR($NAME$.$VARNAME$);\n";
    private const string CALL = "$INSTANCE$(bReset := bReset);\n";
    
    public string Declaration { get; } = "";
    public string Implementation { get; } = "";
    public string Parameter { get; } = "";
    
    public SafetyProgram(IEnumerable<SafetyModule> modules, string pnName)
    {
        foreach (var module in modules.Where(module => module.PnName == pnName && module.IsValid))
        {
            var hstVar = module.HstVariable;
            var devVar = module.DevVariable;
            if (hstVar is null || devVar is null) continue;
            
            Declaration += FB_DECL
                .Replace(Tags.VAR_NAME, module.Name)
                .Replace(Tags.DEVICE, module.BoxName)
                .Replace(Tags.SLOT, module.Slot.ToString())
                .Replace(Tags.SUB_SLOT, module.SubSlot.ToString());
            
            Declaration += hstVar.CreateDeclaration(true, true);
            Declaration += devVar.CreateDeclaration(true, true);
            
            Implementation += CALL
                .Replace(Tags.INSTANCE, module.Name);
            
            Parameter += $"\n{INFO}"
                .Replace(Tags.DEVICE, module.BoxName)
                .Replace(Tags.SLOT, module.Slot.ToString())
                .Replace(Tags.SUB_SLOT, module.SubSlot.ToString());
            Parameter += CONFIG_PORT
                .Replace(Tags.VALUE, module.Port.ToString());
            Parameter += CONFIG_SLOT
                .Replace(Tags.VALUE, module.Slot.ToString());
            Parameter += CONFIG_SUB_SLOT
                .Replace(Tags.VALUE, module.SubSlot.ToString());
            Parameter += CONFIG_HST_SIZE
                .Replace(Tags.VALUE, module.HstSize.ToString());
            Parameter += CONFIG_DEV_SIZE
                .Replace(Tags.VALUE, module.DevSize.ToString());
            Parameter += CONFIG_INPUT_ADDR
                .Replace(Tags.VAR_NAME, module.HstDataName);
            Parameter += CONFIG_OUTPUT_ADDR
                .Replace(Tags.VAR_NAME, module.DevDataName);
            Parameter += CONFIG_HST_DATA_ADDR
                .Replace(Tags.VAR_NAME, module.HstDataName);
            Parameter += CONFIG_DEV_DATA_ADDR
                .Replace(Tags.VAR_NAME, module.DevDataName);
            Parameter = Parameter
                .Replace(Tags.NAME, $"GVL_{pnName}".MakePlcCompatible())
                .Replace(Tags.INSTANCE, module.Name);
        }
    }
}