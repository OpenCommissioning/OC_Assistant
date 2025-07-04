using System.Xml.Linq;

namespace OC.Assistant.Core;

/// <summary>
/// Provides extension methods for the <see cref="XElement"/> class.
/// </summary>
public static class XElementExtension
{
    /// <summary>
    /// Tries to get a child from a parent <see cref="XElement"/>.<br/>
    /// Creates the child if it doesn't exist. 
    /// </summary>
    /// <param name="xElement">The parent <see cref="XElement"/>.</param>
    /// <param name="childName">The name of the child <see cref="XElement"/> to get or create.</param>
    /// <returns>The child <see cref="XElement"/> with the given name.</returns>
    public static XElement GetOrCreateChild(this XElement? xElement, string childName)
    {
        var element = xElement?.Element(childName);
        if (element is not null) return element;
        element = new XElement(childName);
        xElement?.Add(element);
        return element;
    }
    
    /// <summary>
    /// Tries to get a <see cref="XAttribute"/> from a given <see cref="XElement"/>.<br/>
    /// Creates the attribute if it doesn't exist. 
    /// </summary>
    /// <param name="xElement">The parent <see cref="XElement"/>.</param>
    /// <param name="name">The name of the <see cref="XAttribute"/> to get or create.</param>
    /// <returns>The <see cref="XAttribute"/> with the given name.</returns>
    public static XAttribute GetOrCreateAttribute(this XElement? xElement, string name)
    {
        var attribute = xElement?.Attribute(name);
        if (attribute is not null) return attribute;
        attribute = new XAttribute(name, "");
        xElement?.Add(attribute);
        return attribute;
    }
}