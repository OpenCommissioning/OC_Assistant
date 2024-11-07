using System.Reflection;
using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

internal class ParameterCollection : IParameterCollection
{
    private readonly Dictionary<string, IParameter> _fields = new();
    
    public void Add(object owner, FieldInfo? field)
    {
        if (field?.GetCustomAttribute(typeof(PluginParameter)) is not PluginParameter) return;
        var parameter = new Parameter(owner, field);
        
        if (!_fields.ContainsKey(parameter.Name))
        {
            _fields.Add(parameter.Name, parameter);
        }
    } 

    public void Update(XContainer xElement)
    {
        foreach (var parameter in xElement.Elements())
        {
            var name = parameter.Name.ToString();
            if (!_fields.TryGetValue(name, out var field)) return;
            field.Value = parameter.Value;
        }
    }
    
    public void Update(IEnumerable<IParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (!_fields.TryGetValue(parameter.Name, out var field)) continue;
            field.Value = parameter.Value;
        }
    }

    public List<IParameter> ToList()
    {
        return _fields.Values.ToList();
    }

    public XElement XElement
    {
        get
        {
            var xElement = new XElement(nameof(IPluginController.Parameter));
            foreach (var parameter in _fields.Values)
            {
                xElement.Add(new XElement(parameter.Name, parameter.Value));
            }
            return xElement;
        }
    }
}