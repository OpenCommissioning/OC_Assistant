namespace OC.Assistant.Generator;

internal class PouInstanceCall(string? name, string? type, string? init = null, string? comment = null)
{
    private string? Name { get; } = name;
    private string? Type { get; } = type;
    private string? Init { get; } = init;
    private string? Comment { get; } = comment;

    public string DeclarationText
    {
        get
        {
            var declaration = $"\t{Name} : {Type};";
            if (Init != "") declaration = $"\t{Name} : {Type}({Init});";
            if (Comment != "") declaration += $" // {Comment}";
            return declaration + "\n";
        }
    }

    public string ImplementationText => $"\t{Name}();\n";
}