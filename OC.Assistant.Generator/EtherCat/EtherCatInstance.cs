namespace OC.Assistant.Generator.EtherCat;

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
    /// <param name="eCatName">The name of the etherCat simulation.</param>
    /// <param name="name">The instance name.</param>
    /// <param name="type">The instance type.</param>
    /// <param name="template">The etherCat template.</param>
    public EtherCatInstance(string? id, string eCatName, string name, string type, EtherCatTemplate template)
    {
        InstanceName = name;
        
        DeclarationText = template.DeclarationTemplate
            .Replace(Tags.NAME, name)
            .Replace(Tags.BOX_NO, id);
        
        MappingText = template.MappingTemplate
            .Replace(Tags.NAME, name)
            .Replace(Tags.TYPE, type)
            .Replace(Tags.GVL_INSTANCE, $"GVL_{eCatName}.{name}");
        
        CyclicCall = template.CyclicCall;
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
        CyclicCall = false;
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
    /// The plc mapping text.
    /// </summary>
    public string MappingText { get; } = "";
    
    /// <summary>
    /// True when the plc instance is a function block and needs a cyclic call.
    /// </summary>
    public bool CyclicCall { get; }
}