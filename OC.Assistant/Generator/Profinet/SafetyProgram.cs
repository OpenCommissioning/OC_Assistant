using OC.Assistant.Core;

namespace OC.Assistant.Generator.Profinet;

internal class SafetyProgram
{
    private const string TYPE_NAME = "FB_ProfisafeDevice";
    private const string INFO = "\n//Profisafe Device $DEVICE$ - Slot $SLOT$ - SubSlot $SUBSLOT$\n";
    private const string CONFIG_PORT = "$NAME$.$INSTANCE$.stConfig.nPort := $VALUE$;\n";
    private const string CONFIG_SLOT = "$NAME$.$INSTANCE$.stConfig.nSlot := $VALUE$;\n";
    private const string CONFIG_SUB_SLOT = "$NAME$.$INSTANCE$.stConfig.nSubSlot := $VALUE$;\n";
    private const string CONFIG_HST_SIZE = "$NAME$.$INSTANCE$.stConfig.nHstDatasize := $VALUE$;\n";
    private const string CONFIG_DEV_SIZE = "$NAME$.$INSTANCE$.stConfig.nDevDatasize := $VALUE$;\n";
    private const string CONFIG_INPUT_ADDR = "$NAME$.$INSTANCE$.stConfig.pControlData := ADR($NAME$.$VARNAME$);\n";
    private const string CONFIG_OUTPUT_ADDR = "$NAME$.$INSTANCE$.stConfig.pStatusData := ADR($NAME$.$VARNAME$);\n";
    private const string CONFIG_DEV_DATA_ADDR = "$NAME$.$INSTANCE$.stConfig.pDevData := ADR($NAME$.F$VARNAME$);\n";
    private const string CONFIG_HST_DATA_ADDR = "$NAME$.$INSTANCE$.stConfig.pHstData := ADR($NAME$.F$VARNAME$);\n";
    private const string CALL = "$NAME$.$INSTANCE$(bReset := bReset);\n";
    private const string VAR_DECL = "$VARNAME$: $VARTYPE$;\n";

    public string Declaration { get; } = "";
    public string Parameter { get; } = "";
    public string Implementation { get; } = "";

    public SafetyProgram(IEnumerable<SafetyModule> modules, string pnName)
    {
        foreach (var module in modules.Where(module => module.PnName == pnName && module.IsValid))
        {
            Declaration += INFO
                .Replace(Tags.DEVICE, module.BoxName)
                .Replace(Tags.SLOT, module.Slot.ToString())
                .Replace(Tags.SUB_SLOT, module.SubSlot.ToString());
            
            var hstVar = $"F{module.InputAddress} : ARRAY[0..{module.HstSize-1}] OF BYTE;\n"; 
            var devVar = $"F{module.OutputAddress} : ARRAY[0..{module.DevSize-1}] OF BYTE;\n";
            Declaration += hstVar + devVar;

            var name = $"GVL_{pnName}".MakePlcCompatible();
                
            Declaration += VAR_DECL
                .Replace(Tags.VAR_NAME, module.Name)
                .Replace(Tags.VAR_TYPE, TYPE_NAME);
                
            Implementation += CALL
                .Replace(Tags.NAME, name)
                .Replace(Tags.INSTANCE, module.Name);
                
            Parameter += INFO
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
                .Replace(Tags.VAR_NAME, module.InputAddress);
            Parameter += CONFIG_OUTPUT_ADDR
                .Replace(Tags.VAR_NAME, module.OutputAddress);
            Parameter += CONFIG_HST_DATA_ADDR
                .Replace(Tags.VAR_NAME, module.InputAddress);
            Parameter += CONFIG_DEV_DATA_ADDR
                .Replace(Tags.VAR_NAME, module.OutputAddress);
            Parameter = Parameter
                .Replace(Tags.NAME, name)
                .Replace(Tags.INSTANCE, module.Name);
        }
    }
}