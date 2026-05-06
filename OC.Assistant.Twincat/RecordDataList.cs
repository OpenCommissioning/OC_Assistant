using System.Collections.Concurrent;
using OC.Assistant.Sdk;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat;

public static class RecordDataList
{
    private const string PATH = "GVL_Core.refSystem.fbRecordData._aDeviceList";
    private static readonly ConcurrentDictionary<uint, object?> Dictionary = new ();
    
    public static async Task FetchAsync(CancellationToken token)
    {
        try
        {
            Dictionary.Clear();
            using var client = new AdsClient();
            await client.ConnectAsync(new AmsAddress(TcState.Singleton.AmsNetId, TcState.Singleton.PlcPort), token);
            if ((await client.ReadValueAsync(PATH, typeof(uint[]), token)).Value is not uint[] deviceList) return;
            
            foreach (var device in deviceList)
            {
                Dictionary.TryAdd(device, null);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(typeof(TcAdsChannel), e.Message);
        }
    }
    
    public static bool Contains(uint value) => Dictionary.ContainsKey(value);
}