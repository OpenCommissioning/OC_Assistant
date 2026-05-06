namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents a record data structure.
/// </summary>
public struct RecordDataTelegram(ushort identifier, ushort hardwareId, ushort index, uint cbLength, byte[]? data = null)
{
    /// <summary/>
    public readonly ushort Identifier = identifier; 
    /// <summary/>   
    public readonly ushort HardwareId = hardwareId;
    /// <summary/> 
    public readonly ushort Index = index;
    /// <summary/>
    public readonly uint CbLength = cbLength;
    /// <summary/>
    public readonly byte[]? Data = data;
}