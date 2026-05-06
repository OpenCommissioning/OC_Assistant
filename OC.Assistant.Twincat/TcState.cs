using System.Xml.Linq;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat;

public class TcState
{
    private static readonly Lazy<TcState> Lazy = new(() => new TcState());
    
    private readonly AdsClient _adsClient = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private AmsNetId _amsNetId = AmsNetId.Local;
    private ITcSysManager15? _tcSysManager;
    
    public static TcState Singleton => Lazy.Value;

    private TcState()
    {
        if (Lazy.IsValueCreated) return;
        ApiListener.Singleton.AppDisconnected += DisconnectSolution;
        Task.Run(() =>
        {
            try
            {
                _adsClient.Connect((int) AmsPort.R0_Realtime);
                _adsClient.Disconnect();
            }
            catch
            {
                Logger.LogError(this, "Error connecting to the TwinCAT ADS System");
            }
        });
    }
    
    public AdsState AdsState { get; private set; } = AdsState.Idle;
    public AmsNetId AmsNetId { get; private set; } = AmsNetId.Local;
    public int PlcPort { get; private set; } = 851;
    public string? SolutionFullName { get; private set; }
    public event Action<AdsState>? AdsStateChanged;
    
    public void ConnectSolution(string projectFile, string solutionFullName)
    {
        if (SolutionFullName is not null) return;
        SolutionFullName = solutionFullName;
        _cancellationTokenSource = new CancellationTokenSource();
        _amsNetId = GetCurrentNetId();
        AmsNetId = _amsNetId;
        StartPolling(UpdateNetId, 1000);
        StartPolling(UpdateAdsState, 10);
        EventSystem.InvokeAppEvent("app/connect", projectFile);
    }

    private void DisconnectSolution()
    {
        if (SolutionFullName is null) return;
        SolutionFullName = null;
        _cancellationTokenSource.Cancel();
        _adsClient.Disconnect();
        TcDte.ReleaseObject(_tcSysManager);
        _tcSysManager = null;
        AdsState = AdsState.Idle;
        AdsStateChanged?.Invoke(AdsState.Idle);
        EventSystem.InvokeAppEvent("app/disconnect");
    }

    private AdsState GetAdsState()
    {
        try
        {
            if (SolutionFullName is null)
            {
                return AdsState.Idle;
            }

            if (!_adsClient.IsConnected || _amsNetId != AmsNetId)
            {
                Logger.LogInfo(this, $"Setting TwinCAT ADS NetId to {_amsNetId}", true);
                AmsNetId = _amsNetId;
                _adsClient.Disconnect();
                _adsClient.Connect(_amsNetId, (int)AmsPort.R0_Realtime);
                AdsState = AdsState.Idle;
                AdsStateChanged?.Invoke(AdsState.Idle);
                EventSystem.InvokeAppEvent("app/stop", typeof(TcAdsChannel));
            }

            if (_adsClient.TryReadState(out var stateInfo) == AdsErrorCode.NoError)
            {
                return stateInfo.AdsState;
            }
            
            _adsClient.Disconnect();
            return AdsState.Error;
        }
        catch (Exception e)
        {
            Logger.LogError(this, e.Message);
            return AdsState.Exception;
        }
    }

    private AmsNetId GetCurrentNetId()
    {
        try
        {
            if (_tcSysManager is null && SolutionFullName is not null)
            {
                _tcSysManager = TcDte.GetTcSysManager(SolutionFullName);
            }
        }
        catch
        {
            _tcSysManager = null;
        }
        
        try
        {
            var netId = _tcSysManager?.GetTargetNetId();
            return netId is null ? _amsNetId : new AmsNetId(netId);
        }
        catch (Exception e)
        {
            if ((uint)e.HResult != 0x8001010A) //Server is busy; try later
            {
                DisconnectSolution();
            }
        }
        return _amsNetId;
    }

    private void StartPolling(Func<Task> pollingAction, int delayMs)
    {
        var token = _cancellationTokenSource.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await pollingAction().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.LogError(this, $"Polling error: {e.Message}", true);
                }

                await Task.Delay(delayMs, token).ConfigureAwait(false);
            }
        }, token);
    }

    private Task UpdateNetId()
    {
        if (BusyState.IsSet || SolutionFullName is null) return Task.CompletedTask;
        _amsNetId = GetCurrentNetId();
        return Task.CompletedTask;
    }

    private static int GetPlcPort()
    {
        var port = 0;
        DteSingleThread.Run(tcSysManager =>
        {
            if (tcSysManager.GetItem($"{TcShortcut.NODE_PLC_CONFIG}^{XmlFile.Instance.PlcProjectName}") is not {} 
                plc) return;
            var value = XElement
                .Parse(plc.ProduceXml()).Descendants("AdsPort").FirstOrDefault()?.Value;
            port = int.Parse(value ?? "851");
        }, 1000);
        return port;
    }

    private async Task UpdateAdsState()
    {
        if (SolutionFullName is null) return;
        
        var adsState = GetAdsState();
        if (adsState == AdsState) return;
        AdsState = adsState;
        AdsStateChanged?.Invoke(adsState);

        switch (adsState)
        {
            case AdsState.Run:
                PlcPort = GetPlcPort();
                EventSystem.InvokeAppEvent("app/start", typeof(TcAdsChannel));
                TcRecordDataServer.Instance.Connect();
                await RecordDataList.FetchAsync(_cancellationTokenSource.Token);
                break;
            default:
                EventSystem.InvokeAppEvent("app/stop", typeof(TcAdsChannel));
                TcRecordDataServer.Instance.Disconnect();
                break;
        }
    }
}