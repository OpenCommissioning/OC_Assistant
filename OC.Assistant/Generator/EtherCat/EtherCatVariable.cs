using OC.Assistant.Core;
using OC.Assistant.Sdk;

namespace OC.Assistant.Generator.EtherCat;

/// <summary>
/// Class representing a single EtherCAT linked variable.
/// </summary>
/// <param name="name">The name of the variable.</param>
/// <param name="type">The type of the variable.</param>
/// <param name="linkTo">The link information for the 'TcLinkTo' attribute.</param>
internal class EtherCatVariable(string name, string type, string linkTo)
{
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; } = name.MakePlcCompatible();
    
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public string Type { get; } = type.Replace(TcType.Bit.Name(), TcType.Bool.Name());
    
    /// <summary>
    /// The link information for the 'TcLinkTo' attribute.
    /// </summary>
    public string LinkTo { get; } = linkTo;
}