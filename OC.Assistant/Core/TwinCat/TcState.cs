using System.Windows;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using TwinCAT.Ads;

namespace OC.Assistant.Core.TwinCat;

/// <summary>
/// Manages the TwinCAT state.
/// </summary>
public class TcState : TcStateIndicator
{
    private AdsState _lastRunState = AdsState.Idle;
    private ITcSysManager15? _tcSysManager;
    private readonly object _lock = new();
    private readonly AdsClient _adsClient = new();
    private AmsNetId _amsNetId = AmsNetId.Local;
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _adsNotOk;
    
    private bool IsProjectConnected => _tcSysManager is not null;
    
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
                _adsClient.Connect((int)AmsPort.R0_Realtime);
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

    private AdsState GetAdsState()
    {
        lock (_lock)
        {
            try
            {
                if (!IsProjectConnected)
                {
                    return AdsState.Idle;
                }

                if (!_adsClient.IsConnected || _amsNetId != ApiLocal.Interface.NetId)
                {
                    ApiLocal.Interface.NetId = _amsNetId;
                    _adsClient.Disconnect();
                    _adsClient.Connect(_amsNetId, (int)AmsPort.R0_Realtime);
                    IndicateConfigMode();
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
                _adsNotOk = true;
                _cancellationTokenSource.Cancel();
                Logger.LogError(this, e.Message);
                return AdsState.Exception;
            }
        }
    }

    private AmsNetId GetCurrentNetId()
    {
        lock (_lock)
        {
            try
            {
                var netId = _tcSysManager?.GetTargetNetId();
                return netId is null ? _amsNetId : new AmsNetId(netId);
            }
            catch
            {
                return _amsNetId;
            }
        }
    }

    /// <summary>
    /// Is raised when TwinCAT starts running.
    /// </summary>
    public event Action? StartedRunning;

    /// <summary>
    /// Is raised when TwinCAT stops running.
    /// </summary>
    public event Action? StoppedRunning;

    /// <summary>
    /// Connects the <see cref="TcState"/> instance a project.
    /// </summary>
    /// <param name="tcDte">The given <see cref="TcDte"/></param>
    public void ConnectProject(TcDte tcDte)
    {
        if (_adsNotOk) return;

        lock (_lock)
        {
            _tcSysManager = tcDte.GetTcSysManager();
            _amsNetId = GetCurrentNetId();
            ApiLocal.Interface.NetId = _amsNetId;
            _cancellationTokenSource = new CancellationTokenSource();
            SetSolutionPath(tcDte.SolutionFileName);

            StartPolling(UpdateNetId, 2000);
            StartPolling(UpdateAdsState, 100);
        }
    }

    /// <summary>
    /// Disconnects the <see cref="TcState"/> instance from any project.
    /// </summary>
    public void DisconnectProject()
    {
        lock (_lock)
        {
            _cancellationTokenSource.CancelAsync();
            _tcSysManager = null;
            _adsClient.Disconnect();
            IndicateDisconnected();
        }
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
                    // Expected during cancellation, no action needed
                }
                catch (Exception e)
                {
                    Logger.LogError(this, $"Polling error: {e.Message}");
                }

                await Task.Delay(delayMs, token).ConfigureAwait(false);
            }
        }, token);
    }

    private Task UpdateNetId()
    {
        if (!IsProjectConnected) return Task.CompletedTask;
        _amsNetId = GetCurrentNetId();
        return Task.CompletedTask;
    }

    private Task UpdateAdsState()
    {
        if (!IsProjectConnected) return Task.CompletedTask;
        
        var adsState = GetAdsState();
        if (adsState == _lastRunState) return Task.CompletedTask;
        _lastRunState = adsState;

        switch (adsState)
        {
            case AdsState.Run:
                IndicateRunMode();
                ApiLocal.Interface.TriggerTcRestart();
                StartedRunning?.Invoke();
                break;
            default:
                IndicateConfigMode();
                StoppedRunning?.Invoke();
                break;
        }
        
        return Task.CompletedTask;
    }
}