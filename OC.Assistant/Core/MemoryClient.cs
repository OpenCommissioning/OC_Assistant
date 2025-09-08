using System.Collections.Concurrent;
using OC.Assistant.Sdk;

namespace OC.Assistant.Core;

internal class MemoryClient : IClient
{
    public static ConcurrentDictionary<string, byte[]> WriteBuffers { get; } = new();
    public static ConcurrentDictionary<string, byte[]> ReadBuffers { get; } = new();
    
    private string? _writeSymbol;
    private string? _readSymbol;
    
    public byte[] WriteBuffer { get; }
    public byte[] ReadBuffer { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryClient"/>.
    /// </summary>
    /// <param name="writeSize">The size of the write buffer.</param>
    /// <param name="readSize">The size of the read buffer.</param>
    public MemoryClient(int writeSize, int readSize)
    {
        WriteBuffer = new byte[writeSize];
        ReadBuffer = new byte[readSize];
    }
    
    public void Disconnect()
    {
        if (_writeSymbol is not null) WriteBuffers.TryRemove(_writeSymbol, out _);
        if (_readSymbol is not null) ReadBuffers.TryRemove(_readSymbol, out _);
        _writeSymbol = null;
        _readSymbol = null;
    }

    public void SetWriteIndex(string symbolName)
    {
        if (string.IsNullOrEmpty(symbolName))
        {
            Logger.LogError(this, "Write symbol has no value.");
            return;
        }
        _writeSymbol = symbolName;
        WriteBuffers.TryAdd(symbolName, new byte[WriteBuffer.Length]);
    }

    public void SetReadIndex(string symbolName)
    {
        if (string.IsNullOrEmpty(symbolName))
        {
            Logger.LogError(this, "Read symbol has no value.");
            return;
        }
        _readSymbol = symbolName;
        ReadBuffers.TryAdd(symbolName, new byte[ReadBuffer.Length]);
    }

    public void Write()
    {
        try
        {
            if (_writeSymbol is null) return;
            Array.Copy(WriteBuffer, 0, WriteBuffers[_writeSymbol], 0, WriteBuffer.Length);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }

    public void Write(byte[] source, int sourceOffset, int destinationOffset, int length)
    {
        try
        {
            if (_writeSymbol is null) return;
            Array.Copy(source, sourceOffset, WriteBuffers[_writeSymbol], destinationOffset,length);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }

    public void Read()
    {
        try
        {
            if (_readSymbol is null) return;
            Array.Copy(ReadBuffers[_readSymbol], 0, ReadBuffer, 0, ReadBuffer.Length);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }

    public void ReadAll()
    {
        try
        {
            if (_readSymbol is not null)
            {
                Array.Copy(ReadBuffers[_readSymbol], 0, ReadBuffer, 0, ReadBuffer.Length);
            }
            
            if (_writeSymbol is not null)
            {
                Array.Copy(WriteBuffers[_writeSymbol], 0, WriteBuffer, 0, WriteBuffer.Length);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
        }
    }
}