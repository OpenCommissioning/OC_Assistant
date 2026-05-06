using System.Text.RegularExpressions;
using OC.Assistant.Sdk;

namespace OC.Assistant.Twincat;

/// <summary>
/// <see cref="string"/> extension methods.
/// </summary>
public static partial class StringExtension
{
    /// <param name="value">The input string to be converted.</param>
    extension(string value)
    {
        /// <summary>
        /// Removes the description in brackets, e.g. KF2.1 (EL1008) => KF2.1
        /// </summary>
        public string TcRemoveBrackets()
        {
            var regex = BracketsRegex();
            var match = regex.Match(value.Replace(" ", ""));
            return match.Success ? match.Groups[1].ToString() : value;
        }

        /// <summary>
        /// Replaces any character except a-z A-Z and 0-9 with an underscore.<br/>
        /// Underscores at start and end will be removed.
        /// </summary>
        public string TcPlcCompatibleString()
        {
            var str = PlcCharactersRegex().Replace(value, "_");
            str = str.StartsWith('_') ? str.Remove(0, 1) : str;
            str = str.EndsWith('_') ? str.Remove(str.Length - 1) : str;
            return str;
        }
        
        /// Converts the input string to a format compatible with PLC naming conventions.
        /// <return>
        /// The PLC-compatible string. If the input string is already PLC-compatible,
        /// it is returned unchanged; otherwise, it is wrapped with backticks (`).
        /// </return>
        public string MakePlcCompatible() => value.IsBasicCharacters() ? value : $"`{value}`";
        
        /// <summary>
        /// Returns the BitSize of this type if exists, otherwise returns 0.
        /// </summary>
        public int ManagedTypeBitSize()
        {
            var type = value.ToUpper();
            var managedType = TypeByName.GetValueOrDefault(type, TcType.Unknown);
            return managedType == TcType.Unknown ? type.ArrayBitSize() : managedType.BitSize();
        }

        private int ArrayBitSize()
        {
            var match = ArrayRegex().Match(value);
            if (!match.Success) return 0;
            
            var lo = int.Parse(match.Groups[1].Value);
            var hi = int.Parse(match.Groups[2].Value);
            var type = match.Groups[3].Value;
            
            var bitSize = type.ManagedTypeBitSize() * (hi - lo + 1);
            return bitSize > 0 ? bitSize : 0;
        }
    }
    
    private static readonly Dictionary<string, TcType> TypeByName = new()
    {
        { TcType.Bit.Name(), TcType.Bit },
        { TcType.Bool.Name(), TcType.Bool },
        { TcType.Byte.Name(), TcType.Byte },
        { TcType.UsInt.Name(), TcType.UsInt },
        { TcType.SInt.Name(), TcType.SInt },
        { TcType.Word.Name(), TcType.Word },
        { TcType.Uint.Name(), TcType.Uint },
        { TcType.Int.Name(), TcType.Int },
        { TcType.Dword.Name(), TcType.Dword },
        { TcType.UDint.Name(), TcType.UDint },
        { TcType.Dint.Name(), TcType.Dint },
        { TcType.Real.Name(), TcType.Real },
        { TcType.LWord.Name(), TcType.LWord },
        { TcType.Lint.Name(), TcType.Lint },
        { TcType.ULint.Name(), TcType.ULint },
        { TcType.LReal.Name(), TcType.LReal }
    };
    
    [GeneratedRegex(@"(\S+)\(\S+\)", RegexOptions.IgnoreCase)]
    private static partial Regex BracketsRegex();
    
    [GeneratedRegex("[^a-zA-Z0-9]+")]
    private static partial Regex PlcCharactersRegex();
    
    [GeneratedRegex(@"^ARRAY\s*\[(\d+)..(\d+)]\s*OF\s*(\S+)$", RegexOptions.IgnoreCase, "de-DE")]
    private static partial Regex ArrayRegex();
}