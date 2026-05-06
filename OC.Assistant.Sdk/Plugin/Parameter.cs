using System.Reflection;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents a plugin parameter. 
/// </summary>
internal class Parameter : IParameter
{
    private readonly object _owner;
    private readonly FieldInfo _field;
    
    /// <summary>
    /// Creates a new instance of the <see cref="Parameter"/> class.
    /// </summary>
    public Parameter(object owner, FieldInfo field)
    {
        _owner = owner;
        _field = field;
        Name = field.Name.Replace("_", "").FirstCharToUpper();
        if (field.GetCustomAttribute<PluginParameter>() is not {} attribute) return;
        ToolTip = attribute.ToolTip;
        if (field.FieldType != typeof(bool)) FileFilter = attribute.FileFilter;
    }
    
    /// <summary>
    /// The parameter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The parameter value.
    /// </summary>
    public object? Value
    {
        get => _field.GetValue(_owner);
        set => _field.SetValue(_owner, value?.ConvertTo(_field.FieldType));
    }
    
    /// <summary>
    /// The parameter tooltip, if any.
    /// </summary>
    public object? ToolTip { get; }
    
    /// <summary>
    /// The file filter, if any.
    /// </summary>
    public string? FileFilter { get; }
}