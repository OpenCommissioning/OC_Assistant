using System.Text.RegularExpressions;

namespace OC.Assistant.Core;

/// <summary>
/// Provides extension methods for string manipulation tailored for PLC compatibility.
/// This static class contains helper methods to validate and modify strings to comply with specific PLC requirements.
/// </summary>
public static partial class StringExtension
{
    [GeneratedRegex("[^a-zA-Z0-9_]+")]
    private static partial Regex InvalidCharacters();
    
    /// Converts the input string to a format compatible with PLC naming conventions.
    /// <param name="input">The input string to be converted.</param>
    /// <return>Returns the PLC-compatible string. If the input string is already PLC-compatible, it is returned unchanged; otherwise, it is wrapped with backticks (`).</return>
    public static string MakePlcCompatible(this string input)
    {
        return input.IsPlcCompatible() ? input : $"`{input}`";
    }

    /// <summary>
    /// Checks if the input string is compatible with PLC naming conventions.
    /// </summary>
    /// <param name="input">The input string to verify.</param>
    /// <returns>Returns <c>true</c> if the input string is PLC compatible; otherwise, <c>false</c>.</returns>
    public static bool IsPlcCompatible(this string input)
    {
        return !string.IsNullOrEmpty(input) && !InvalidCharacters().IsMatch(input);
    }
}