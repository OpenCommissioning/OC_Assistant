using System.Text.RegularExpressions;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat.Automation.EtherCat;

/// <summary>
/// Class representing a single EtherCAT linked variable.
/// </summary>
/// <param name="name">The name of the variable.</param>
/// <param name="type">The type of the variable.</param>
/// <param name="linkTo">The link information for the 'TcLinkTo' attribute.</param>
internal partial class EtherCatVariable(string name, string type, string linkTo)
{
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; } = name.TcPlcCompatibleString();
    
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public string Type { get; } = GetValidType(type);
    
    /// <summary>
    /// The link information for the 'TcLinkTo' attribute.
    /// </summary>
    public string LinkTo { get; } = linkTo;

    private static string GetValidType(string type)
    {
        return RegexGetBit().Replace(type, match =>
        {
            if (!int.TryParse(match.Groups[1].Value, out var number))
            {
                return match.Value;
            }
                
            return number switch
            {
                > 0 and <= 8 => TcType.Byte.Name(),
                > 8 and <= 16 => TcType.Word.Name(),
                > 16 and <= 32 => TcType.Dword.Name(),
                > 32 and <= 64 => TcType.LWord.Name(),
                _ => match.Value
            };
        }).Replace(TcType.Bit.Name(), TcType.Bool.Name());
    }

    [GeneratedRegex(@"\b(?:bool|bit)(\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexGetBit();
}