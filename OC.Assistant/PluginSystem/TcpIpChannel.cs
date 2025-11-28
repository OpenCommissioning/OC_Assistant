using System.Collections.Concurrent;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.PluginSystem;

internal class TcpIpChannel(int writeSize, int readSize) : ChannelBase(writeSize, readSize)
{
    public static ConcurrentDictionary<string, byte[]> WriteBuffers { get; } = new();
    public static ConcurrentDictionary<string, byte[]> ReadBuffers { get; } = new();
    
    private string? _writeSymbol;
    private string? _readSymbol;

    public override void Disconnect()
    {
        if (_writeSymbol is not null) WriteBuffers.TryRemove(_writeSymbol, out _);
        if (_readSymbol is not null) ReadBuffers.TryRemove(_readSymbol, out _);
        _writeSymbol = null;
        _readSymbol = null;
    }

    public override void SetWriteIndex(string name, string? suffix)
    {
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError(this, "Write symbol has no value.");
            return;
        }
        _writeSymbol = name;
        WriteBuffers.TryAdd(name, new byte[WriteBuffer.Length]);
    }

    public override void SetReadIndex(string name, string? suffix)
    {
        if (string.IsNullOrEmpty(name))
        {
            Logger.LogError(this, "Read symbol has no value.");
            return;
        }
        _readSymbol = name;
        ReadBuffers.TryAdd(name, new byte[ReadBuffer.Length]);
    }

    public override void Write()
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

    public override void Write(byte[] source, int sourceOffset, int destinationOffset, int length)
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

    public override void Read()
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

    public override void ReadAll()
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

    public override IRecordDataServer RecordDataServer { get; } = RecordData.Instance;
    public override double TimeScaling => AppControl.Instance.TimeScaling;
    public override string ServerAddress => AppControl.Instance.Settings.IpAddress;
    public override int ServerPort => AppControl.Instance.Settings.PluginServerPort;
}