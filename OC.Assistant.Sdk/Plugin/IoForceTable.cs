namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Thread-safe table of forced values.
/// Modifications replace an immutable snapshot, so <see cref="Apply"/> never blocks the plugin cycle.
/// </summary>
internal class IoForceTable : IIoForceTable
{
    private readonly record struct Entry(int BitOffset, int BitSize, byte[] Value);

    private readonly Lock _lock = new();
    private readonly Dictionary<int, Entry> _entries = new();
    private volatile Entry[] _snapshot = [];

    public int Count => _snapshot.Length;

    public void Set(int bitOffset, int bitSize, byte[] value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bitOffset);

        if (bitSize != 1 && (bitSize <= 0 || bitSize % 8 != 0 || bitOffset % 8 != 0))
        {
            throw new ArgumentException("Invalid bit size or offset.");
        }

        var length = bitSize == 1 ? 1 : bitSize / 8;
        if (value.Length < length)
        {
            throw new ArgumentException($"Value must have at least {length} bytes.", nameof(value));
        }

        var entry = new Entry(bitOffset, bitSize, value[..length]);

        lock (_lock)
        {
            _entries[bitOffset] = entry;
            _snapshot = _entries.Values.ToArray();
        }
    }

    public void Remove(int bitOffset)
    {
        lock (_lock)
        {
            if (!_entries.Remove(bitOffset)) return;
            _snapshot = _entries.Values.ToArray();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
            _snapshot = [];
        }
    }

    /// <summary>
    /// Writes all forced values to the given buffer.
    /// </summary>
    public void Apply(byte[] buffer)
    {
        foreach (var entry in _snapshot)
        {
            var byteOffset = entry.BitOffset / 8;

            if (entry.BitSize == 1)
            {
                if (byteOffset >= buffer.Length) continue;
                var mask = (byte)(1 << (entry.BitOffset % 8));
                if ((entry.Value[0] & 1) != 0) buffer[byteOffset] |= mask;
                else buffer[byteOffset] &= (byte)~mask;
                continue;
            }

            if (byteOffset + entry.Value.Length > buffer.Length) continue;
            entry.Value.CopyTo(buffer, byteOffset);
        }
    }
}
