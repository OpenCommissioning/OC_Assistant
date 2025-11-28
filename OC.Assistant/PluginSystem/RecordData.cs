using System.Collections.Concurrent;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.PluginSystem;

public class RecordData : IRecordDataServer
{
    private static readonly Lazy<RecordData> Lazy = new(() => new RecordData());
    private ConcurrentDictionary<uint, Queue<RecordDataTelegram>> WriteRequests { get; } = new();
    private ConcurrentDictionary<uint, Queue<RecordDataTelegram>> ReadRequests { get; } = new();
    private ConcurrentDictionary<uint, object?> SubscribedDevices { get; } = new();
    
    public static RecordData Instance => Lazy.Value;
    
    private RecordData()
    {
    }

    public RecordDataTelegram? TryGetWriteRequest(ushort identifier, ushort hardWareId)
    {
        var key = (uint) (hardWareId + 0x10000 * identifier);
        return WriteRequests.TryGetValue(key, out var queue) && queue.Count > 0 ? queue.Dequeue() : null;
    }

    public RecordDataTelegram? TryGetReadRequest(ushort identifier, ushort hardWareId)
    {
        var key = (uint) (hardWareId + 0x10000 * identifier);
        return ReadRequests.TryGetValue(key, out var queue) && queue.Count > 0 ? queue.Dequeue() : null;
    }

    public void Subscribe(ushort identifier, ushort hardWareId)
    {
        var key = (uint) (hardWareId + 0x10000 * identifier);
        SubscribedDevices.TryAdd(key, null);
    }

    private static RecordDataTelegram CreateResponse(ushort identifier, ushort hardWareId, ushort index, uint dataLength, byte[]? data = null)
        => new(identifier, hardWareId, index, dataLength, data);
    
    public void SendWriteRes(ushort identifier, ushort hardWareId, ushort index, uint dataLength)
        => OnWriteRes?.Invoke(CreateResponse(identifier, hardWareId, index, dataLength));
    
    public void SendReadRes(ushort identifier, ushort hardWareId, ushort index, uint dataLength, byte[] data)
        => OnReadRes?.Invoke(CreateResponse(identifier, hardWareId, index, dataLength, data));
    
    public void WriteReq(RecordDataTelegram request)
    {
        var key = (uint) (request.HardwareId + 0x10000 * request.Identifier);
        if (!SubscribedDevices.ContainsKey(key)) return;
        WriteRequests.TryAdd(key, new Queue<RecordDataTelegram>());
        WriteRequests[key].Enqueue(request);
    }

    public void ReadReq(RecordDataTelegram request)
    {
        var key = (uint) (request.HardwareId + 0x10000 * request.Identifier);
        if (!SubscribedDevices.ContainsKey(key)) return;
        ReadRequests.TryAdd(key, new Queue<RecordDataTelegram>());
        ReadRequests[key].Enqueue(request);
    }

    public event Action<RecordDataTelegram>? OnWriteRes;
    public event Action<RecordDataTelegram>? OnReadRes;
}