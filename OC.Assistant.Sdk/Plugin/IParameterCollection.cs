using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents the interface for a parameter collection.
/// </summary>
public interface IParameterCollection
{
    /// <summary>
    /// Updates the parameter collection with the given XML element.
    /// </summary>
    /// <param name="xElement"></param>
    public void Update(XContainer xElement);

    /// <summary>
    /// Updates the parameter collection with the given parameters.
    /// </summary>
    /// <param name="parameters"></param>
    public void Update(IEnumerable<IParameter> parameters);

    /// <summary>
    /// Converts the parameter collection to a list.
    /// </summary>
    /// <returns></returns>
    public List<IParameter> ToList();

    /// <summary>
    /// Converts the parameter collection to an XML element.
    /// </summary>
    public XElement ToXElement();
}