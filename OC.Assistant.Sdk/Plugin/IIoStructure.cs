using System.Xml.Linq;

namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Interface to define the input- or output-structure for a plugin.
/// </summary>
public interface IIoStructure
{
    /// <summary>
    /// Size in bytes of the current structure.
    /// </summary>
    public int Length { get; }
    
    /// <summary>
    /// Adds a variable to the structure.
    /// </summary>
    /// <param name="name">Name of the variable.</param>
    /// <param name="type">Type of the variable.</param>
    /// <param name="arraySize">No array when zero, otherwise size of the array.</param>
    public void AddVariable(string name, TcType type, int arraySize = 0);
    
    /// <summary>
    /// The current structure in xml-format.
    /// </summary>
    internal XElement XElement { get; }
    
    /// <summary>
    /// Removes all variables.
    /// </summary>
    internal void Clear();
}