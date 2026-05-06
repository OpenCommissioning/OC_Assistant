using System.Reflection;
using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents a parameter collection.
/// </summary>
internal class ParameterCollection : IParameterCollection
{
    private readonly Dictionary<string, IParameter> _fields = new();
    
    internal void Add(object owner, FieldInfo? field)
    {
        if (field?.GetCustomAttribute(typeof(PluginParameter)) is not PluginParameter) return;
        var parameter = new Parameter(owner, field);
        
        _fields.TryAdd(parameter.Name, parameter);
    } 

    /// <summary>
    /// Updates the parameter collection with the given XML element.
    /// </summary>
    /// <param name="xElement"></param>
    public void Update(XContainer xElement)
    {
        foreach (var parameter in xElement.Elements())
        {
            var name = parameter.Name.ToString();
            if (!_fields.TryGetValue(name, out var field)) return;
            field.Value = field is {FileFilter: "Password", Value: string} ? parameter.Value.Decrypt() : parameter.Value;
        }
    }
    
    /// <summary>
    /// Updates the parameter collection with the given parameters.
    /// </summary>
    /// <param name="parameters"></param>
    public void Update(IEnumerable<IParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (!_fields.TryGetValue(parameter.Name, out var field)) continue;
            field.Value = parameter.Value;
        }
    }

    /// <summary>
    /// Converts the parameter collection to a list.
    /// </summary>
    /// <returns></returns>
    public List<IParameter> ToList()
    {
        return _fields.Values.ToList();
    }

    /// <summary>
    /// Converts the parameter collection to an XML element.
    /// </summary>
    public XElement ToXElement()
    {
        var xElement = new XElement(nameof(IPluginController.Parameter));
        foreach (var parameter in _fields.Values)
        {
            var value = parameter is {FileFilter: "Password", Value: string pw} ? pw.Encrypt() : parameter.Value;
            xElement.Add(new XElement(parameter.Name, value));
        }
        return xElement;
    }
}