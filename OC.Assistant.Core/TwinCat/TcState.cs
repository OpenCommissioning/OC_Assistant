using System.Windows;
using OC.Assistant.Sdk;
using TwinCAT.Ads;

namespace OC.Assistant.Core.TwinCat;

public class TcState : TcStateIndicator
{
    private bool _wasRunning;
    private bool _isProjectConnected;
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
                if (!_adsClient.IsConnected)
                {
                    return false;
                }
                return _adsClient.TryReadState(out var stateInfo) == AdsErrorCode.NoError && stateInfo.AdsState == AdsState.Run;
            }
            catch
            {
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
        await Task.Run(() =>
        {
            try
            {
                BusyState.Set(this);
                Logger.LogInfo(this, "Connecting to TwinCAT.Ads...");
                _adsClient.Connect((int) AmsPort.R0_Realtime);
                _wasRunning = IsRunning;
                _adsClient.RouterStateChanged += AdsClientOnRouterStateChanged;
                Logger.LogInfo(this, "TwinCAT.Ads successfully connected");
            }
            catch (Exception exception)
            {
                Logger.LogError(this, exception.Message);
            }
            finally
            {
                BusyState.Reset(this);
            }
        });
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
                IndicateDisconnected();
                return;
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
}