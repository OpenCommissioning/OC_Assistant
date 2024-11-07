using System.Text.RegularExpressions;

namespace OC.Assistant.Sdk;

/// <summary>
/// <see cref="string"/> extension methods.
/// </summary>
internal static class StringExtension
{
    /// <summary>
    /// Converts a string of numbers (e.g. 1,2,3,4,16-32,64-128) into a list.
    /// </summary>
    public static int[] ToNumberList(this string value)
    {
        var res = new List<int>();
        const string pattern = @"(?<val>((?<low>\d+)\s*?-\s*?(?<high>\d+))|(\d+))";

        foreach (Match m in Regex.Matches(value, pattern))
        {
            if (!m.Groups["val"].Success) continue;
                
            //found area (e.g. 16-32)
            if (m.Groups["low"].Success && m.Groups["high"].Success)
            {
                var low = Convert.ToInt32(m.Groups["low"].Value);
                var high = Convert.ToInt32(m.Groups["high"].Value);
                for (var i = low; i <= high; i++)
                {
                    res.Add(i);
                }
                continue; //next match
            }
            res.Add(Convert.ToInt32(m.Groups["val"].Value)); //single value

        }
        return res.ToArray();
    }

    /// <summary>
    /// Removes the description in brackets, e.g. KF2.1 (EL1008) => KF2.1
    /// </summary>
    public static string TcRemoveBrackets(this string value)
    {
        const string pattern = @"(\S+)\(\S+\)";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        var match = regex.Match(value.Replace(" ", ""));
        return match.Success ? match.Groups[1].ToString() : value;
    }
        
    /// <summary>
    /// Replaces any character except a-z A-Z and 0-9 with a underscore.<br/>
    /// Underscores at start and end will be removed.
    /// </summary>
    public static string TcPlcCompatibleString(this string value)
    {
        var str =  Regex.Replace(value, "[^a-zA-Z0-9]+", "_");
        str = str.StartsWith("_") ? str.Remove(0, 1) : str;
        str = str.EndsWith("_") ? str.Remove(str.Length - 1) : str;
        return str;
    }
        
    /// <summary>
    /// Converts a string of hex values (e.g. 0xFF, 03-02-01, ef cd ab) into a byte array.
    /// </summary>
    public static byte[] ToByteArray(this string value)
    {
        //Remove unnecessary characters
        var hex = value.Replace(" ", "").Replace("0x", "").Replace("-", "");
            
        //Check if length is an odd number an fill with '0'
        if (hex.Length % 2 != 0) hex = hex.PadLeft(hex.Length + 1, '0');
            
        //Declare a byte array with half the string size, e.g. '01' equals 1 byte
        var bytes = new byte[hex.Length / 2];
            
        //Convert the substrings byte by byte
        for (var i = 0; i < hex.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            
        //Reverse the array and return
        Array.Reverse(bytes);
        return bytes;
    }
        
    /// <summary>
    /// Returns the string with the first letter capitalized.
    /// </summary>
    public static string FirstCharToUpper(this string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Length == 1 ? $"{char.ToUpper(value[0])}" : $"{char.ToUpper(value[0])}{value.Substring(1)}";
    }
}