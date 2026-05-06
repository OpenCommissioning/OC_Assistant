using System.Diagnostics;
using OC.Assistant.Sdk;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.TypeSystem;

namespace OC.Assistant.Twincat;

/// <summary>
/// Extensions and helpers for the <see cref="TwinCAT.Ads.AdsClient"/> class.
/// </summary>
internal static class TcAdsClientExtension
{
    extension(AdsClient client)
    {
        /// <summary>
        /// Gets the ADS index information for the given symbol.
        /// </summary>
        /// <param name="symbolName">The symbol name.</param>
        /// <param name="adsSymbolLoader">The provided <see cref="IAdsSymbolLoader"/>, if any.</param>
        /// <returns>The <see cref="TcAdsIndex"/> for the given symbol.</returns>
        /// <exception cref="Exception">Symbol not found.</exception>
        public TcAdsIndex GetAdsIndex(string symbolName, IAdsSymbolLoader? adsSymbolLoader = null)
        {
            var symbolLoaderSettings = new SymbolLoaderSettings(SymbolsLoadMode.Flat);
            var symbolLoader = adsSymbolLoader ?? (IAdsSymbolLoader) SymbolLoaderFactory.Create(client, symbolLoaderSettings);
        
            foreach (var symbol in symbolLoader.Symbols)
            {
                var adsSymbol = (IAdsSymbol) symbol;
                if (adsSymbol.InstancePath != symbolName) continue;
                Logger.LogInfo(typeof(TcAdsClientExtension), $"Symbol '{symbolName}' connected", true);
                return new TcAdsIndex(adsSymbol.IndexGroup, adsSymbol.IndexOffset);
            }
            
            throw new Exception($"Symbol '{symbolName}' not found or TwinCAT is not running");
        }
        
        private bool ConnectSystemService()
        {
            client.Connect(TcState.Singleton.AmsNetId, (int)AmsPort.SystemService);
            return client.ReadAdsState() != AdsState.Error;
        }

        private AdsState ReadAdsState()
        {
            var adsErrorCode = client.TryReadState(out var stateInfo);
            if (adsErrorCode == AdsErrorCode.NoError) return stateInfo.AdsState;
            Logger.LogError(typeof(TcAdsClientExtension), $"Ads error: {adsErrorCode}");
            return AdsState.Error;
        }

        private async Task WaitForState(AdsState adsState, long millisecondsTimeout = 10000)
        {
            var timeout = false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (client.ReadAdsState() != adsState && !timeout)
            {
                timeout = stopwatch.ElapsedMilliseconds > millisecondsTimeout;
                if (timeout)
                {
                    Logger.LogError(typeof(TcAdsClientExtension), $"Timeout for TwinCAT state {adsState}");
                }
                await Task.Delay(100);
            }
        }
    }
    
    /// <summary>
    /// Starts TwinCAT.
    /// </summary>
    public static async Task Start(long millisecondsTimeout = 10000)
    {
        var client = new AdsClient();
        if (!client.ConnectSystemService()) return;
        if (client.ReadAdsState() == AdsState.Run) return;
        client.WriteControl(new StateInfo(AdsState.Reset, 0));
        await client.WaitForState(AdsState.Run, millisecondsTimeout);
        client.Close();
    }

    /// <summary>
    /// Restarts TwinCAT in config mode.
    /// </summary>
    public static async Task ConfigMode(long millisecondsTimeout = 10000)
    {
        var client = new AdsClient();
        if (!client.ConnectSystemService()) return;
        if (client.ReadAdsState() == AdsState.Config) return;
        client.WriteControl(new StateInfo(AdsState.Reconfig, 0));
        await client.WaitForState(AdsState.Config, millisecondsTimeout);
        client.Close();
    }
}