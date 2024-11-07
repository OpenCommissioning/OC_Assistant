using System.Text.RegularExpressions;

namespace OC.Assistant.Sdk;

/// <summary>
/// Static class with <see cref="TcType"/> extensions.
/// </summary>
public static class TcTypeExtension
{
    /// <summary>
    /// Returns a managed string of this <see cref="TcType"/>.
    /// </summary>
    public static string Name(this TcType type)
    {
        return type.ToString().ToUpper();
    }
        
    /// <summary>
    /// Returns the BitSize of this <see cref="TcType"/>.
    /// </summary>
    public static int BitSize(this TcType type)
    {
        return BitSizeByType[type];
    }
        
    /// <summary>
    /// Converts this <see cref="TcType"/>
    /// to a <see cref="T:System.Type"/>. Default is <see cref="T:System.Byte"/>[].
    /// </summary>
    public static Type ToSystemType(this TcType type)
    {
        return type switch
        {
            TcType.Bit or TcType.Bool => typeof(bool),
            TcType.Byte or TcType.UsInt or TcType.SInt => typeof(byte),
            TcType.Uint => typeof(ushort),
            TcType.Int => typeof(short),
            TcType.UDint => typeof(uint),
            TcType.Dint => typeof(int),
            TcType.ULint => typeof(ulong),
            TcType.Lint => typeof(long),
            TcType.Real => typeof(float),
            TcType.LReal => typeof(double),
            _ => typeof(byte[])
        };
    }
        
    /// <summary>
    /// Converts the <see cref="T:System.String"/>
    /// to a <see cref="TcType"/> if possible.
    /// </summary>
    public static TcType ToTcType(this string typeName)
    {
        return TypeByName.TryGetValue(typeName, out var value) ? value : TcType.Unknown;
    }
        
    /// <summary>
    /// Returns the BitSize of this type if exists, otherwise returns 0.
    /// </summary>
    public static int TcBitSize(this string type)
    {
        return type.ToTcType() == TcType.Unknown ? TcArrayBitSize(type) : BitSizeByType[type.ToTcType()];
    }

    private static int TcArrayBitSize(this string arrayType)
    {
        const string pattern = @"^ARRAY\s*\[(\d+)..(\d+)]\s*OF\s*(\S+)$";
        var match = Regex.Match(arrayType, pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return 0;
            
        var lo = int.Parse(match.Groups[1].Value);
        var hi = int.Parse(match.Groups[2].Value);
        var type = match.Groups[3].Value;
            
        var bitSize = type.TcBitSize() * (hi - lo + 1);
        return bitSize > 0 ? bitSize : 0;
    }
        
    private static readonly Dictionary<TcType, int> BitSizeByType = new()
    {
        { TcType.Bit, 1 },
        { TcType.Bool, 8 },
        { TcType.Byte, 8 },
        { TcType.UsInt, 8 },
        { TcType.SInt, 8 },
        { TcType.Word, 16 },
        { TcType.Uint, 16 },
        { TcType.Int, 16 },
        { TcType.Dword, 32 },
        { TcType.UDint, 32 },
        { TcType.Dint, 32 },
        { TcType.Real, 32 },
        { TcType.LWord, 64 },
        { TcType.Lint, 64 },
        { TcType.ULint, 64 },
        { TcType.LReal, 64 },
        { TcType.Unknown, 0 },
    };
        
    private static readonly Dictionary<string, TcType> TypeByName = new()
    {
        { Name(TcType.Bit), TcType.Bit },
        { Name(TcType.Bool), TcType.Bool },
        { Name(TcType.Byte), TcType.Byte },
        { Name(TcType.UsInt), TcType.UsInt },
        { Name(TcType.SInt), TcType.SInt },
        { Name(TcType.Word), TcType.Word },
        { Name(TcType.Uint), TcType.Uint },
        { Name(TcType.Int), TcType.Int },
        { Name(TcType.Dword), TcType.Dword },
        { Name(TcType.UDint), TcType.UDint },
        { Name(TcType.Dint), TcType.Dint },
        { Name(TcType.Real), TcType.Real },
        { Name(TcType.LWord), TcType.LWord },
        { Name(TcType.Lint), TcType.Lint },
        { Name(TcType.ULint), TcType.ULint },
        { Name(TcType.LReal), TcType.LReal }
    };
}