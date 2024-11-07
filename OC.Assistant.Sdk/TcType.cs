namespace OC.Assistant.Sdk;

/// <summary>
/// TwinCAT basic types.
/// </summary>
public enum TcType
{
    /// <summary>Represents a 1-bit <see cref="System.Boolean"/>.</summary>
    Bit,
    /// <summary>Represents a 8-bit <see cref="System.Boolean"/>.</summary>
    Bool,
    /// <summary>Represents a 8-bit <see cref="System.Byte"/>.</summary>
    Byte,
    /// <summary>Represents a 8-bit <see cref="System.Byte"/>.</summary>
    UsInt,
    /// <summary>Represents a 8-bit <see cref="System.Byte"/>.</summary>
    SInt,
    /// <summary>Represents an 16-bit <see cref="System.UInt16"/>.</summary>
    Word,
    /// <summary>Represents an 16-bit <see cref="System.UInt16"/>.</summary>
    Uint,
    /// <summary>Represents an 16-bit <see cref="System.Int16"/>.</summary>
    Int,
    /// <summary>Represents an 32-bit <see cref="System.UInt32"/>.</summary>
    Dword,
    /// <summary>Represents an 32-bit <see cref="System.UInt32"/>.</summary>
    UDint,
    /// <summary>Represents an 32-bit <see cref="System.Int32"/>.</summary>
    Dint,
    /// <summary>Represents a 32-bit <see cref="System.Single"/>.</summary>
    Real,
    /// <summary>Represents an 64-bit <see cref="System.UInt64"/>.</summary>
    LWord,
    /// <summary>Represents an 64-bit <see cref="System.Int64"/>.</summary>
    Lint,
    /// <summary>Represents an 64-bit <see cref="System.UInt64"/>.</summary>
    ULint,
    /// <summary>Represents a 64-bit <see cref="System.Double"/>.</summary>
    LReal,
    /// <summary>Not supported</summary>
    Unknown
}