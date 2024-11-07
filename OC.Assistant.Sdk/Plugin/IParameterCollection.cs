using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

internal interface IParameterCollection
{
    void Update(XContainer xElement);
    void Update(IEnumerable<IParameter> parameter);
    List<IParameter> ToList();
    XElement XElement { get; }
}