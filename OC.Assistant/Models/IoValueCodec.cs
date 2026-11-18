using System.Buffers.Binary;
using System.Globalization;
using OC.Assistant.Sdk;

namespace OC.Assistant.Models;

/// <summary>
/// Converts plugin buffer data to and from strings, based on the <see cref="TcType"/>.
/// </summary>
public static class IoValueCodec
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    /// <summary>
    /// Reads the value at the given bit offset and returns it as string.
    /// Returns an empty string if the buffer is too small.
    /// </summary>
    public static string Read(byte[] buffer, int bitOffset, TcType type)
    {
        var offset = bitOffset / 8;
        var size = type == TcType.Bit ? 1 : type.BitSize() / 8;
        if (offset + size > buffer.Length) return "";
        var span = buffer.AsSpan(offset, size);

        return type switch
        {
            TcType.Bit => FormatBool((span[0] & (1 << (bitOffset % 8))) != 0),
            TcType.Bool => FormatBool(span[0] != 0),
            TcType.Byte or TcType.UsInt => span[0].ToString(Culture),
            TcType.SInt => ((sbyte)span[0]).ToString(Culture),
            TcType.Word or TcType.Uint => BinaryPrimitives.ReadUInt16LittleEndian(span).ToString(Culture),
            TcType.Int => BinaryPrimitives.ReadInt16LittleEndian(span).ToString(Culture),
            TcType.Dword or TcType.UDint => BinaryPrimitives.ReadUInt32LittleEndian(span).ToString(Culture),
            TcType.Dint => BinaryPrimitives.ReadInt32LittleEndian(span).ToString(Culture),
            TcType.Real => BinaryPrimitives.ReadSingleLittleEndian(span).ToString(Culture),
            TcType.LWord or TcType.ULint => BinaryPrimitives.ReadUInt64LittleEndian(span).ToString(Culture),
            TcType.Lint => BinaryPrimitives.ReadInt64LittleEndian(span).ToString(Culture),
            TcType.LReal => BinaryPrimitives.ReadDoubleLittleEndian(span).ToString(Culture),
            _ => ""
        };
    }

    /// <summary>
    /// Parses the given string to little-endian bytes of the given type.
    /// Integers can be given in decimal or hexadecimal (0x...) format.
    /// </summary>
    public static bool TryParse(string? text, TcType type, out byte[] value)
    {
        value = [];
        text = text?.Trim();
        if (string.IsNullOrEmpty(text)) return false;

        switch (type)
        {
            case TcType.Bit or TcType.Bool:
                if (!TryParseBool(text, out var b)) return false;
                value = [(byte)(b ? 1 : 0)];
                return true;
            case TcType.Real:
                if (!float.TryParse(text, NumberStyles.Float, Culture, out var f)) return false;
                value = new byte[4];
                BinaryPrimitives.WriteSingleLittleEndian(value, f);
                return true;
            case TcType.LReal:
                if (!double.TryParse(text, NumberStyles.Float, Culture, out var d)) return false;
                value = new byte[8];
                BinaryPrimitives.WriteDoubleLittleEndian(value, d);
                return true;
        }

        var size = type.BitSize() / 8;
        if (size == 0) return false;
        var signed = type is TcType.SInt or TcType.Int or TcType.Dint or TcType.Lint;
        if (!TryParseInteger(text, signed, size, out var raw)) return false;

        value = new byte[size];
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, raw);
        bytes[..size].CopyTo(value);
        return true;
    }

    private static string FormatBool(bool value) => value ? "TRUE" : "FALSE";

    private static bool TryParseBool(string text, out bool value)
    {
        switch (text.ToUpperInvariant())
        {
            case "TRUE" or "1":
                value = true;
                return true;
            case "FALSE" or "0":
                value = false;
                return true;
            default:
                value = false;
                return false;
        }
    }

    private static bool TryParseInteger(string text, bool signed, int size, out ulong raw)
    {
        raw = 0;
        var bits = size * 8;

        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith("16#", StringComparison.Ordinal))
        {
            var hex = (text.StartsWith("16#") ? text[3..] : text[2..]).Replace("_", "");
            if (!ulong.TryParse(hex, NumberStyles.AllowHexSpecifier, Culture, out raw)) return false;
            return bits == 64 || raw < 1UL << bits;
        }

        if (signed)
        {
            if (!long.TryParse(text, NumberStyles.Integer, Culture, out var l)) return false;
            var min = bits == 64 ? long.MinValue : -(1L << (bits - 1));
            var max = bits == 64 ? long.MaxValue : (1L << (bits - 1)) - 1;
            if (l < min || l > max) return false;
            raw = unchecked((ulong)l);
            return true;
        }

        if (!ulong.TryParse(text, NumberStyles.None, Culture, out raw)) return false;
        return bits == 64 || raw < 1UL << bits;
    }
}
