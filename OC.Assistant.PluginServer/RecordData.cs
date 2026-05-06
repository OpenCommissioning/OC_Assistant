using System.Collections.Concurrent;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.PluginServer;

public class RecordData : IRecordDataServer
{
    private static readonly Lazy<RecordData> Lazy = new(() => new RecordData());
    private ConcurrentDictionary<uint, ConcurrentQueue<RecordDataTelegram>> WriteRequests { get; } = new();
    private ConcurrentDictionary<uint, ConcurrentQueue<RecordDataTelegram>> ReadRequests { get; } = new();
    private ConcurrentDictionary<uint, object?> SubscribedDevices { get; } = new();
    
    public static RecordData Instance => Lazy.Value;
    
    private RecordData()
    {
    }

    public RecordDataTelegram? TryGetWriteRequest(ushort identifier, ushort hardWareId)
    {
        var key = hardWareId | (uint)(identifier << 16);
        return WriteRequests.TryGetValue(key, out var queue) && !queue.IsEmpty ? 
            queue.TryDequeue(out var telegram) ? telegram : null : null;
    }

    public RecordDataTelegram? TryGetReadRequest(ushort identifier, ushort hardWareId)
    {
        var key = hardWareId | (uint)(identifier << 16);
        return ReadRequests.TryGetValue(key, out var queue) && !queue.IsEmpty ? 
            queue.TryDequeue(out var telegram) ? telegram : null : null;
    }

    public void Subscribe(ushort identifier, ushort hardWareId)
    {
        var key = hardWareId | (uint)(identifier << 16);
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
        var key = request.HardwareId | (uint)(request.Identifier << 16);
        if (!SubscribedDevices.ContainsKey(key)) return;
        WriteRequests.TryAdd(key, new ConcurrentQueue<RecordDataTelegram>());
        WriteRequests[key].Enqueue(request);
    }

    public void ReadReq(RecordDataTelegram request)
    {
        var key = request.HardwareId | (uint)(request.Identifier << 16);
        if (!SubscribedDevices.ContainsKey(key)) return;
        ReadRequests.TryAdd(key, new ConcurrentQueue<RecordDataTelegram>());
        ReadRequests[key].Enqueue(request);
    }

    public event Action<RecordDataTelegram>? OnWriteRes;
    public event Action<RecordDataTelegram>? OnReadRes;
}