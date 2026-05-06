namespace OC.Assistant.Sdk.Plugin;

/// <summary>
/// Represents an interface for a record data server.
/// </summary>
public interface IRecordDataServer
{
    /// <summary>
    /// Sends a write-request.
    /// </summary>
    /// <param name="request">The <see cref="RecordDataTelegram"/> containing the request data.</param>
    public void WriteReq(RecordDataTelegram request);
    
    /// <summary>
    /// Sends a read-request.
    /// </summary>
    /// <param name="request">The <see cref="RecordDataTelegram"/> containing the request data.</param>
    public void ReadReq(RecordDataTelegram request);
    
    /// <summary>
    /// Is raised whenever a write operation is completed,
    /// providing a <see cref="RecordDataTelegram"/> as its argument.
    /// </summary>
    public event Action<RecordDataTelegram>? OnWriteRes;
    
    /// <summary>
    /// Is raised whenever a read operation is completed,
    /// providing a <see cref="RecordDataTelegram"/> as its argument.
    /// </summary>
    public event Action<RecordDataTelegram>? OnReadRes;
}