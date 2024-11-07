using TwinCAT.Ads;

namespace OC.Assistant.Sdk;

/// <summary>
/// Class to locate RecordData-subscribed devices from TwinCAT. 
/// </summary>
public class TcRecordDataList
{
    private readonly HashSet<uint> _list = [];
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new instance of the <see cref="TcRecordDataList"/> class.
    /// </summary>
    public TcRecordDataList()
    {
        Fetch();
        ApiLocal.Interface.TcRestart += Fetch;
    }
    
    private void Fetch()
    {
        Task.Run(() =>
        {
            try
            {
                using var client = new AdsClient();
                client.Connect(851);
                var deviceList = (uint[]) client.ReadValue("GVL_Core.refSystem.fbRecordData.aDeviceList");

                lock (_lock)
                {
                    _list.Clear();
                    foreach (var device in deviceList)
                    {
                        _list.Add(device);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
            }
        });
    }

    /// <summary>
    /// Determines whether the list contains the specified value.
    /// </summary>
    /// <param name="value">The value to locate.</param>
    /// <returns>True if the list contains the specified value, otherwise false.</returns>
    public bool Contains(uint value)
    {
        lock (_lock)
        {
            return _list.Contains(value);
        }
    }
}