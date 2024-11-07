using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace OC.Assistant.Sdk;

/// <summary>
/// Custom TcAdsClient with an underlying <see cref="TwinCAT.Ads.AdsClient"/>.
/// </summary>
internal class TcAdsClient
{
    private readonly AdsClient _adsClient;
    private IAdsSymbolLoader AdsSymbolLoader { get; }
    private TcAdsIndex _writeIndex;
    private TcAdsIndex _readIndex;
    private readonly byte[] _combinedBuffer;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="TcAdsClient"/>.
    /// </summary>
    /// <param name="port">The ADS port to connect to.</param>
    /// <param name="writeSize">The size of the write buffer.</param>
    /// <param name="readSize">The size of the read buffer.</param>
    public TcAdsClient(int port, int writeSize, int readSize)
    {
        _adsClient = new AdsClient();
        _adsClient.Connect(port);
        
        AdsSymbolLoader = (IAdsSymbolLoader)SymbolLoaderFactory
            .Create(_adsClient, new SymbolLoaderSettings(SymbolsLoadMode.Flat));
        
        WriteBuffer = new byte[writeSize];
        ReadBuffer = new byte[readSize];
        _combinedBuffer = new byte[readSize + writeSize];
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
    public void Disconnect()
    {
        try
        {
            _adsClient.Disconnect();
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }
    
    /// <summary>
    /// Sets the write index to the given symbol.
    /// </summary>
    /// <param name="symbolName">The symbol name.</param>
    public void SetWriteIndex(string symbolName)
    {
        if (WriteBuffer.Length == 0) return;
        
        try
        {
            _writeIndex = _adsClient.GetAdsIndex(symbolName, AdsSymbolLoader);
        }
        catch (Exception e)
        {
            Logger.LogWarning(this, e.Message);
        }
    }

    /// <summary>
    /// Sets the read index to the given symbol.
    /// </summary>
    /// <param name="symbolName">The symbol name.</param>
    public void SetReadIndex(string symbolName)
    {
        if (ReadBuffer.Length == 0) return;
        
        try
        {
            _readIndex = _adsClient.GetAdsIndex(symbolName, AdsSymbolLoader);
        }
        catch (Exception e)
        {
            Logger.LogWarning(this, e.Message);
        }
    }

    /// <summary>
    /// Writes data from the <see cref="WriteBuffer"/> to TwinCAT.
    /// </summary>
    public void Write()
    {
        if (!_writeIndex.Valid) return;

        try
        {
            _adsClient.Write(_writeIndex.IndexGroup, _writeIndex.IndexOffset, new ReadOnlyMemory<byte>(WriteBuffer));
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }
    
    /// <summary>
    /// Writes data from a custom source to TwinCAT.
    /// </summary>
    /// <param name="source">The source data.</param>
    /// <param name="sourceOffset">The source offset.</param>
    /// <param name="destinationOffset">The TwinCAT relative offset.</param>
    /// <param name="length">Data length.</param>
    public void Write(byte[] source, int sourceOffset, int destinationOffset, int length)
    {
        if (!_writeIndex.Valid) return;
        
        if (destinationOffset + length > WriteBuffer.Length)
        {
            Logger.LogWarning(this, "Write command denied. Out of destination range.");
            return;
        }
        
        try
        {
            _adsClient.Write(_writeIndex.IndexGroup, 
                _writeIndex.IndexOffset + (uint)destinationOffset,
                new Memory<byte>(source).Slice(sourceOffset, length));
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }

    /// <summary>
    /// Reads data from TwinCAT and copies to the <see cref="ReadBuffer"/>.
    /// </summary>
    public void Read()
    {
        if (!_readIndex.Valid) return;

        try
        {
            _adsClient.Read(_readIndex.IndexGroup, _readIndex.IndexOffset, new Memory<byte>(ReadBuffer));
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }
    
    /// <summary>
    /// Reads data from the TwinCAT read- and write buffer.
    /// </summary>
    public void ReadAll()
    {
        if (!_readIndex.Valid && !_writeIndex.Valid) return;
        
        try
        {
            if (!_readIndex.Valid)
            {
                _adsClient.Read(_writeIndex.IndexGroup, _writeIndex.IndexOffset, new Memory<byte>(WriteBuffer));
                return;
            }

            if (!_writeIndex.Valid)
            {
                Read();
                return;
            }
            
            _adsClient.Read(_readIndex.IndexGroup, _readIndex.IndexOffset, new Memory<byte>(_combinedBuffer));
            Array.Copy(_combinedBuffer, 0, ReadBuffer, 0, ReadBuffer.Length);
            Array.Copy(_combinedBuffer, ReadBuffer.Length, WriteBuffer, 0, WriteBuffer.Length);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }
}