namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// The base for a plugin channel.
/// </summary>
public abstract class ChannelBase
{
    /// <summary>
    /// Creates a new instance of the <see cref="ChannelBase"/>.
    /// </summary>
    /// <param name="writeSize">The size of the write buffer.</param>
    /// <param name="readSize">The size of the read buffer.</param>
    protected ChannelBase(int writeSize, int readSize)
    {
        WriteBuffer = new byte[writeSize];
        ReadBuffer = new byte[readSize];
    }
    
    /// <summary>
    /// Buffer to store data to write.
    /// </summary>
    public byte[] WriteBuffer { get; }
    
    /// <summary>
    /// Buffer with read data.
    /// </summary>
    public byte[] ReadBuffer { get; }
    
    /// <summary>
    /// Disconnects the client.
    /// </summary>
    public abstract void Disconnect();
    
    /// <summary>
    /// Sets the write-index to the given name.
    /// </summary>
    /// <param name="name">The plugin name.</param>
    /// <param name="suffix">Optional suffix for the name.</param>
    public abstract void SetWriteIndex(string name, string? suffix);
    
    /// <summary>
    /// Sets the read-index to the given name.
    /// </summary>
    /// <param name="name">The plugin name.</param>
    /// <param name="suffix">Optional suffix for the name.</param>
    public abstract void SetReadIndex(string name, string? suffix);
    
    /// <summary>
    /// Writes data from the <see cref="WriteBuffer"/> to the server.
    /// </summary>
    public abstract void Write();
    
    /// <summary>
    /// Writes data from a custom source to the server.
    /// </summary>
    /// <param name="source">The source data.</param>
    /// <param name="sourceOffset">The source offset.</param>
    /// <param name="destinationOffset">The server relative offset.</param>
    /// <param name="length">Data length.</param>
    public abstract void Write(byte[] source, int sourceOffset, int destinationOffset, int length);
    
    /// <summary>
    /// Reads data from the server and copies to the <see cref="ReadBuffer"/>.
    /// </summary>
    public abstract void Read();
    
    /// <summary>
    /// Reads data from the server read- and write buffer.
    /// </summary>
    public abstract void ReadAll();
    
    /// <summary>
    /// The interface for the record data server.
    /// </summary>
    public abstract IRecordDataServer RecordDataServer { get; }
}