namespace OC.Assistant.Sdk;

/// <summary>
/// Static class with <see cref="TcType"/> extensions.
/// </summary>
public static class TcTypeExtension
{
    /// <param name="type">The <see cref="TcType"/> to extend.</param>
    extension(TcType type)
    {
        /// <summary>
        /// Returns a managed string of this <see cref="TcType"/>.
        /// </summary>
        public string Name()
        {
            return type.ToString().ToUpper();
        }

        /// <summary>
        /// Returns the BitSize of this <see cref="TcType"/>.
        /// </summary>
        public int BitSize()
        {
            return BitSizeByType[type];
        }
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
        { TcType.Unknown, 0 }
    };
}