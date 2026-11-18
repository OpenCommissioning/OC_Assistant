namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Interface for a table of forced values.
/// A forced value overwrites the related part of an input- or output-buffer every cycle.
/// </summary>
public interface IIoForceTable
{
    /// <summary>
    /// Forces a value.
    /// </summary>
    /// <param name="bitOffset">The offset in bits relative to the start of the buffer.
    /// Must be a multiple of 8 when <paramref name="bitSize"/> is not 1.</param>
    /// <param name="bitSize">The size in bits. Either 1 or a multiple of 8.</param>
    /// <param name="value">The value in little-endian byte order.
    /// For a single bit, the least significant bit of the first byte is used.</param>
    void Set(int bitOffset, int bitSize, byte[] value);

    /// <summary>
    /// Removes the forced value at the given offset.
    /// </summary>
    /// <param name="bitOffset">The offset in bits relative to the start of the buffer.</param>
    void Remove(int bitOffset);

    /// <summary>
    /// Removes all forced values.
    /// </summary>
    void Clear();

    /// <summary>
    /// The number of forced values.
    /// </summary>
    int Count { get; }
}
