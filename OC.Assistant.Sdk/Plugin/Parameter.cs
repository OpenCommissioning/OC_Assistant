using System.Reflection;

namespace OC.Assistant.Sdk.Plugin;

internal class Parameter : IParameter
{
    private readonly object _owner;
    private readonly FieldInfo _field;
    
    public string Name { get; }

    public object? Value
    {
        get => _field.GetValue(_owner);
        set => _field.SetValue(_owner, value?.ConvertTo(_field.FieldType));
    }
    
    public object? ToolTip { get; }
    public string? FileFilter { get; }
    
    public Parameter(object owner, FieldInfo field)
    {
        _owner = owner;
        _field = field;
        Name = field.Name.Replace("_", "").FirstCharToUpper();
        if (field.GetCustomAttribute(typeof(PluginParameter)) is not PluginParameter attribute) return;
        ToolTip = attribute.ToolTip;
        if (field.FieldType != typeof(bool)) FileFilter = attribute.FileFilter;
    }
}