using System.Windows;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using TwinCAT.Ads;

namespace OC.Assistant.Core.TwinCat;

public class TcState : TcStateIndicator
{
    private bool _lastRunState;
    private ITcSysManager15? _tcSysManager;
    private readonly AdsClient _adsClient = new ();
    private AmsNetId _amsNetId = AmsNetId.Local;
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _adsNotOk;
    
    private bool IsProjectConnected => _tcSysManager is not null;
    
    private bool CurrentRunState
    {
        get
        {
            try
            {
                if (!IsProjectConnected)
                {
                    return false;
                }

                var netId = _amsNetId;
                if (!_adsClient.IsConnected || netId != ApiLocal.Interface.NetId)
                {
                    ApiLocal.Interface.NetId = netId;
                    _adsClient.Connect(netId, (int) AmsPort.R0_Realtime);
                    IndicateConfigMode();
                }
                
                switch (_adsClient.TryReadState(out var stateInfo))
                {
                    case AdsErrorCode.NoError:
                        return stateInfo.AdsState == AdsState.Run;
                    default:
                        _lastRunState = false;
                        _adsClient.Disconnect();
                        return false;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message);
                return false;
            }
        }
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="TcState"/> class.
    /// </summary>
    public TcState()
    {
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Run(() =>
            {
                BusyState.Set(this);
                Logger.LogInfo(this, "Checking TwinCAT ADS server...");
                _adsClient.Connect((int) AmsPort.R0_Realtime);
                _adsClient.Disconnect();
                Logger.LogInfo(this, "TwinCAT ADS server ok");
            });
        }
        catch (Exception exception)
        {
            _adsNotOk = true;
            Logger.LogError(this, exception.Message);
        }
        finally
        {
            BusyState.Reset(this);
        }
    }

    private AmsNetId GetCurrentNetId()
    {
        var netId = Retry.Invoke(() => _tcSysManager?.GetTargetNetId(), 120000);
        return netId is null ? AmsNetId.Local : new AmsNetId(netId);
    }

    /// <summary>
    /// Is raised when TwinCAT started running.
    /// </summary>
    public event Action? StartedRunning;

    /// <summary>
    /// Is raised when TwinCAT stopped running.
    /// </summary>
    public event Action? StoppedRunning;
    
    public void ConnectProject(TcDte tcDte)
    {
        if (_adsNotOk) return;
        
        _tcSysManager = tcDte.GetTcSysManager();
        ApiLocal.Interface.NetId = GetCurrentNetId();
        _lastRunState = CurrentRunState;
        _amsNetId = GetCurrentNetId();
        _cancellationTokenSource = new CancellationTokenSource();

        StartPollingNetId();
        StartPollingAdsState();

        if (!CurrentRunState)
        {
            StoppedRunning?.Invoke();
            IndicateConfigMode();
            return;
        }
            
        StartedRunning?.Invoke();
        IndicateRunMode();
    }
    
    public void DisconnectProject()
    {
        _cancellationTokenSource.CancelAsync();
        _tcSysManager = null;
        _adsClient.Disconnect();
        IndicateDisconnected();
    }

    private void StartPollingNetId()
    {
        var token = _cancellationTokenSource.Token;
        
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(2000, token);
                
                if (!IsProjectConnected)
                {
                    return;
                }
                
                _amsNetId = GetCurrentNetId();
            }
        }, token);
    }
    
    private void StartPollingAdsState()
    {
        var token = _cancellationTokenSource.Token;
        
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100, token);
                
                if (!IsProjectConnected)
                {
                    return;
                }
        
                switch (CurrentRunState)
                {
                    case true when !_lastRunState:
                        _lastRunState = true;
                        IndicateRunMode();
                        ApiLocal.Interface.TriggerTcRestart();
                        StartedRunning?.Invoke();
                        continue;
                    case false when _lastRunState:
                        _lastRunState = false;
                        IndicateConfigMode();
                        StoppedRunning?.Invoke();
                        continue;
                    default:
                        continue;
                }
            }
        }, token);
    }
}