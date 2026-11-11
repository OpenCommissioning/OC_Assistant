namespace OC.Assistant.Twincat.Automation.EtherCat;

/// <summary>
/// Class representing the plc instance of a EtherCAT linked variable,
/// structure or function block.
/// </summary>
internal class EtherCatInstance
{
    /// <summary>
    /// Constructor for template.
    /// </summary>
    /// <param name="id">The box id of the etherCat device.</param>
    /// <param name="name">The instance name.</param>
    /// <param name="template">The etherCat template.</param>
    public EtherCatInstance(string? id, string name, EtherCatTemplate template)
    {
        InstanceName = name;
        
        DeclarationText = template.DeclarationTemplate
            .Replace(Tags.NAME, name)
            .Replace(Tags.BOX_NO, id);
        
        CyclicCallInstances = template.CyclicCallSuffixes
            .Select(suffix => $"{name}{suffix}")
            .ToArray();
    }

    /// <summary>
    /// Constructor for generic variable.
    /// </summary>
    /// <param name="variable">Information of a single etherCat variable.</param>
    public EtherCatInstance(EtherCatVariable variable)
    {
        InstanceName = variable.Name;
        var attribute = $"{{attribute 'TcLinkTo' := '{variable.LinkTo}'}}";
        DeclarationText = $"{attribute}\n{variable.Name} {variable.Type};";
        CyclicCallInstances = [];
    }

    /// <summary>
    /// The plc instance name.
    /// </summary>
    public string InstanceName { get; }
    
    /// <summary>
    /// The plc declaration text including attributes.
    /// </summary>
    public string DeclarationText { get; }
    
    /// <summary>
    /// PLC function-block instance names that need cyclic calls.
    /// </summary>
    public IReadOnlyList<string> CyclicCallInstances { get; }
}