using System.Collections.Concurrent;
using OC.Assistant.Sdk;

namespace OC.Assistant.Plugins;

public class RecordData : IRecordDataServer
{
    private static readonly Lazy<RecordData> Lazy = new(() => new RecordData());
    private ConcurrentDictionary<uint, Queue<RecordDataRequest>> WriteRequests { get; } = new();
    private ConcurrentDictionary<uint, Queue<RecordDataRequest>> ReadRequests { get; } = new();
    
    public static RecordData Instance => Lazy.Value;
    
    private RecordData()
    {
    }
    
    public RecordDataRequest? TryGetWriteRequest(uint indexOffset)
        => WriteRequests.TryGetValue(indexOffset, out var queue) && queue.Count > 0 ? queue.Dequeue() : null;
    
    public RecordDataRequest? TryGetReadRequest(uint indexOffset)
        => ReadRequests.TryGetValue(indexOffset, out var queue) && queue.Count > 0 ? queue.Dequeue() : null;

    private static RecordDataResponse CreateResponse(uint indexOffset, uint index, uint dataLength, byte[]? data = null)
    {
        var identifier = indexOffset >> 16;
        var hardwareId = (ushort)indexOffset;
        var invokeId = index * 0x10000 + hardwareId;
        var result = (int)(0x80000000 + identifier);
        return new RecordDataResponse(invokeId, result, dataLength, data);
    }
    
    public void SendWriteRes(uint indexOffset, uint index, uint dataLength)
        => OnWriteRes?.Invoke(CreateResponse(indexOffset, index, dataLength));
    
    public void SendReadRes(uint indexOffset, uint index, uint dataLength, byte[] data)
        => OnReadRes?.Invoke(CreateResponse(indexOffset, index, dataLength, data));
    
    public void WriteReq(RecordDataRequest request)
    {
        WriteRequests.TryAdd(request.IndexOffset, new Queue<RecordDataRequest>());
        WriteRequests[request.IndexOffset].Enqueue(request);
    }

    public void ReadReq(RecordDataRequest request)
    {
        ReadRequests.TryAdd(request.IndexOffset, new Queue<RecordDataRequest>());
        ReadRequests[request.IndexOffset].Enqueue(request);
    }

    public event Action<RecordDataResponse>? OnWriteRes;
    public event Action<RecordDataResponse>? OnReadRes;
}