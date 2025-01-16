namespace OC.Assistant.Generator.Profinet;

internal class SafetyProgram
{
    private const string TYPE_NAME = "FB_ProfisafeDevice";
    private const string INFO = "\n//Profisafe Device $DEVICE$ - Slot $SLOT$ - SubSlot $SUBSLOT$\n";
    private const string CONFIG_PORT = "GVL_$NAME$.$INSTANCE$.stConfig.stRecordIdent.nPort := $VALUE$;\n";
    private const string CONFIG_SLOT = "GVL_$NAME$.$INSTANCE$.stConfig.stRecordIdent.nSlot := $VALUE$;\n";
    private const string CONFIG_SUB_SLOT = "GVL_$NAME$.$INSTANCE$.stConfig.stRecordIdent.nSubSlot := $VALUE$;\n";
    private const string CONFIG_HST_SIZE = "GVL_$NAME$.$INSTANCE$.stConfig.nHstDatasize := $VALUE$;\n";
    private const string CONFIG_DEV_SIZE = "GVL_$NAME$.$INSTANCE$.stConfig.nDevDatasize := $VALUE$;\n";
    private const string CONFIG_INPUT_ADDR = "GVL_$NAME$.$INSTANCE$.stConfig.pFromBus := ADR(GVL_$NAME$.$VARNAME$);\n";
    private const string CONFIG_OUTPUT_ADDR = "GVL_$NAME$.$INSTANCE$.stConfig.pToBus := ADR(GVL_$NAME$.$VARNAME$);\n";
    private const string CONFIG_DEV_DATA_ADDR = "GVL_$NAME$.$INSTANCE$.stConfig.pDevUserData := ADR(GVL_$NAME$.$VARNAME$);\n";
    private const string CONFIG_HST_DATA_ADDR = "GVL_$NAME$.$INSTANCE$.stConfig.pHstUserData := ADR(GVL_$NAME$.$VARNAME$);\n";
    private const string CALL = "GVL_$NAME$.$INSTANCE$(bReset := bReset);\n";
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
                
            Declaration += VAR_DECL
                .Replace(Tags.VAR_NAME, module.Name)
                .Replace(Tags.VAR_TYPE, TYPE_NAME);
                
            Implementation += CALL
                .Replace(Tags.NAME, pnName)
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
                .Replace(Tags.NAME, pnName)
                .Replace(Tags.INSTANCE, module.Name);
        }
    }
}