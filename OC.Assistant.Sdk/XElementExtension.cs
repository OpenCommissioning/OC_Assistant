using System.Xml.Linq;

namespace OC.Assistant.Sdk;

/// <summary>
/// Provides extension methods for the <see cref="XElement"/> class.
/// </summary>
public static class XElementExtension
{
    /// <param name="xElement">The parent <see cref="XElement"/>.</param>
    extension(XElement? xElement)
    {
        /// <summary>
        /// Tries to get a child from a parent <see cref="XElement"/>.<br/>
        /// Creates the child if it doesn't exist. 
        /// </summary>
        /// <param name="childName">The name of the child <see cref="XElement"/> to get or create.</param>
        /// <param name="defaultValue">The default value of the child <see cref="XElement"/>.</param>
        /// <returns>The child <see cref="XElement"/> with the given name.</returns>
        public XElement GetOrCreateChild(string childName, object? defaultValue = null)
        {
            var element = xElement?.Element(childName);
            if (element is not null) return element;
            element = new XElement(childName, defaultValue);
            xElement?.Add(element);
            return element;
        }

        /// <summary>
        /// Tries to get a <see cref="XAttribute"/> from a given <see cref="XElement"/>.<br/>
        /// Creates the attribute if it doesn't exist. 
        /// </summary>
        /// <param name="name">The name of the <see cref="XAttribute"/> to get or create.</param>
        /// /// <param name="defaultValue">The default value <see cref="XAttribute"/>.</param>
        /// <returns>The <see cref="XAttribute"/> with the given name.</returns>
        public XAttribute GetOrCreateAttribute(string name, object? defaultValue = null)
        {
            var attribute = xElement?.Attribute(name);
            if (attribute is not null) return attribute;
            attribute = new XAttribute(name, defaultValue ?? string.Empty);
            xElement?.Add(attribute);
            return attribute;
        }
    }
}