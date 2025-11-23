using System.Text.RegularExpressions;
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
    private const string REGEX_BOOL = @"\b(?:bool|bit)(\d+)\b";
    
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; } = name.TcPlcCompatibleString();
    
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public string Type => GetValidType();

    /// <summary>
    /// The link information for the 'TcLinkTo' attribute.
    /// </summary>
    public string LinkTo { get; } = linkTo;

    private string GetValidType()
    {
        var result = type.Replace(TcType.Bit.Name(), TcType.Bool.Name());
        
        result = Regex.Replace(result, REGEX_BOOL, match =>
        {
            var number = int.Parse(match.Groups[1].Value);

            return number switch
            {
                >= 0 and <= 7 => "BYTE",
                >= 8 and <= 15 => "WORD",
                >= 16 and <= 31 => "DWORD",
                _ => match.Value
            };
        },
            RegexOptions.IgnoreCase      
        );

        return result;
    }
}