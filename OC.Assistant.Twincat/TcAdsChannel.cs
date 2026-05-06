using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace OC.Assistant.Twincat;

/// <summary>
/// Custom TcAdsClient with an underlying <see cref="TwinCAT.Ads.AdsClient"/>.
/// </summary>
public class TcAdsChannel : ChannelBase
{
    private readonly AdsClient _adsClient;
    private IAdsSymbolLoader AdsSymbolLoader { get; }
    private TcAdsIndex _writeIndex;
    private TcAdsIndex _readIndex;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="TcAdsChannel"/>.
    /// </summary>
    /// <param name="writeSize">The size of the write buffer.</param>
    /// <param name="readSize">The size of the read buffer.</param>
    public TcAdsChannel(int writeSize, int readSize) : base(writeSize, readSize)
    {
        _adsClient = new AdsClient();
        _adsClient.Connect(TcState.Singleton.AmsNetId, TcState.Singleton.PlcPort);
        
        AdsSymbolLoader = (IAdsSymbolLoader)SymbolLoaderFactory
            .Create(_adsClient, new SymbolLoaderSettings(SymbolsLoadMode.Flat));
    }

    /// <summary>
    /// Disconnects the client.
    /// </summary>
    public override void Disconnect()
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
    
    public override void SetWriteIndex(string name, string? suffix)
    {
        if (WriteBuffer.Length == 0) return;
        
        try
        {
            _writeIndex = _adsClient.GetAdsIndex($"GVL_{name}.{suffix}", AdsSymbolLoader);
        }
        catch (Exception e)
        {
            Logger.LogWarning(this, e.Message);
        }
    }
    
    public override void SetReadIndex(string name, string? suffix)
    {
        if (ReadBuffer.Length == 0) return;
        
        try
        {
            _readIndex = _adsClient.GetAdsIndex($"GVL_{name}.{suffix}", AdsSymbolLoader);
        }
        catch (Exception e)
        {
            Logger.LogWarning(this, e.Message);
        }
    }

    /// <summary>
    /// Writes data from the <see cref="ChannelBase.WriteBuffer"/> to TwinCAT.
    /// </summary>
    public override void Write()
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
    public override void Write(byte[] source, int sourceOffset, int destinationOffset, int length)
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
    /// Reads data from TwinCAT and copies to the <see cref="ChannelBase.ReadBuffer"/>.
    /// </summary>
    public override void Read()
    {
        if (!_readIndex.Valid) return;

        try
        {
            _adsClient
                .ReadAsResult(_readIndex.IndexGroup, _readIndex.IndexOffset, ReadBuffer.Length)
                .Data
                .CopyTo(ReadBuffer);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }
    
    /// <summary>
    /// Reads data from the TwinCAT read- and write buffer.
    /// </summary>
    public override void ReadAll()
    {
        if (!_readIndex.Valid && !_writeIndex.Valid) return;
        
        try
        {
            if (!_readIndex.Valid)
            {
                _adsClient
                    .ReadAsResult(_writeIndex.IndexGroup, _writeIndex.IndexOffset, WriteBuffer.Length)
                    .Data
                    .CopyTo(WriteBuffer);
                return;
            }

            if (!_writeIndex.Valid)
            {
                Read();
                return;
            }
            
            var readBytes = _adsClient
                .ReadAsResult(_readIndex.IndexGroup, _readIndex.IndexOffset, ReadBuffer.Length + WriteBuffer.Length);
            
            readBytes.Data[..ReadBuffer.Length].CopyTo(ReadBuffer);
            readBytes.Data[ReadBuffer.Length..].CopyTo(WriteBuffer);
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message, true);
        }
    }

    public override IRecordDataServer RecordDataServer => TcRecordDataServer.Instance;
}