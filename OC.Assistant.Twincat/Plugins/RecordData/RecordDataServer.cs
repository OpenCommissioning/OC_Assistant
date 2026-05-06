using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;

namespace OC.Assistant.Twincat.Plugins.RecordData;

public class RecordDataServer : PluginBase
{
    [PluginParameter("ADS Port\nDefault 852")]
    private readonly ushort _port = 852;

    private PnAdsServer? _adsServer;

    protected override bool OnSave()
    {
        return true;
    }

    protected override bool OnStart()
    {
        if (ChannelType != typeof(TcAdsChannel))
        {
            Logger.LogError(this, $"This plugin can only run on {nameof(TcAdsChannel)}");
            CancellationRequest();
            return false;
        }
        
        _adsServer = new PnAdsServer(_port);
        return true;
    }

    protected override void OnUpdate()
    {
    }

    protected override void OnStop()
    {
        _adsServer?.Disconnect();
    }
}