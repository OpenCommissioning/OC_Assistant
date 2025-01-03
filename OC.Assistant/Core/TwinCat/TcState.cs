using System.Diagnostics;
using System.Windows;
using OC.Assistant.Sdk;
using TwinCAT.Ads;

namespace OC.Assistant.Core.TwinCat;

public class TcState : TcStateIndicator
{
    private bool _wasRunning;
    private bool _isProjectConnected;
    private AdsErrorCode _adsErrorCode;
    private readonly AdsClient _adsClient = new ();

    /// <summary>
    /// Gets a value if the TwinCAT system is in <see cref="AdsState.Run"/>.
    /// </summary>
    public bool IsRunning
    {
        get
        {
            try
            {
                if (!IsProjectConnected)
                {
                    return false;
                }
                
                if (!_adsClient.IsConnected)
                {
                    _adsClient.Connect(ApiLocal.Interface.NetId, (int) AmsPort.R0_Realtime);
                    Logger.LogInfo(this, $"Connected to TwinCAT NetId {ApiLocal.Interface.NetId}");
                }

                var adsErrorCode = _adsClient.TryReadState(out var stateInfo);
                
                switch (adsErrorCode)
                {
                    case AdsErrorCode.NoError:
                        _adsErrorCode = adsErrorCode;
                        return stateInfo.AdsState == AdsState.Run;
                    case AdsErrorCode.TargetPortNotFound:
                        _adsErrorCode = adsErrorCode;
                        _adsClient.Disconnect();
                        _adsClient.Connect(ApiLocal.Interface.NetId, (int) AmsPort.R0_Realtime);
                        return _adsClient.TryReadState(out stateInfo) == AdsErrorCode.NoError && stateInfo.AdsState == AdsState.Run;
                    default:
                        if (_adsErrorCode == adsErrorCode)
                        {
                            return false;
                        }
                        _adsErrorCode = adsErrorCode;
                        Logger.LogError(this, $"Could not read TwinCAT state on {ApiLocal.Interface.NetId}. Error: {adsErrorCode}");
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
            Logger.LogError(this, exception.Message);
        }
        finally
        {
            BusyState.Reset(this);
        }
    }

    /// <summary>
    /// Is raised when TwinCAT started running.
    /// </summary>
    public event Action? StartedRunning;

    /// <summary>
    /// Is raised when TwinCAT stopped running.
    /// </summary>
    public event Action? StoppedRunning;
    
    /// <summary>
    /// Gets or sets if a project is connected.
    /// </summary>
    public bool IsProjectConnected
    {
        get => _isProjectConnected;
        set
        {
            _isProjectConnected = value;
            if (!value)
            {
                _adsClient.Disconnect();
                _adsClient.RouterStateChanged -= AdsClientOnRouterStateChanged;
                IndicateDisconnected();
                return;
            }
            
            _wasRunning = IsRunning;

            if (ApiLocal.Interface.NetId == AmsNetId.Local)
            {
                _adsClient.RouterStateChanged += AdsClientOnRouterStateChanged;
            }
            
            if (ApiLocal.Interface.NetId != AmsNetId.Local)
            {
                StartPolling();
            }

            if (!IsRunning)
            {
                StoppedRunning?.Invoke();
                IndicateConfigMode();
                return;
            }
            
            StartedRunning?.Invoke();
            IndicateRunMode();
        }
    }

    private void AdsClientOnRouterStateChanged(object? sender, AmsRouterNotificationEventArgs e)
    {
        if (e.State == AmsRouterState.Start && IsRunning && !_wasRunning)
        {
            _wasRunning = true;

            if (!IsProjectConnected)
            {
                return;
            }
            
            IndicateRunMode();
            ApiLocal.Interface.TriggerTcRestart();
            StartedRunning?.Invoke();
            return;
        }

        if (!_wasRunning)
        {
            return;
        }

        _wasRunning = false;

        if (!IsProjectConnected)
        {
            return;
        }
        
        IndicateConfigMode();
        StoppedRunning?.Invoke();
    }

    private void StartPolling()
    {
        var stopwatch = new Stopwatch();
        
        Task.Run(() =>
        {
            while (IsProjectConnected)
            {
                stopwatch.WaitUntil(100);

                if (!IsProjectConnected)
                {
                    return;
                }

                if (IsRunning && !_wasRunning)
                {
                    AdsClientOnRouterStateChanged(this, new AmsRouterNotificationEventArgs(AmsRouterState.Start));
                    continue;
                }
                
                if (!IsRunning && _wasRunning)
                {
                    AdsClientOnRouterStateChanged(this, new AmsRouterNotificationEventArgs(AmsRouterState.Stop));
                }
            }
        });
    }
}