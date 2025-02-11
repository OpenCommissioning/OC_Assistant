using System.Windows;
using EnvDTE;
using OC.Assistant.Controls;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using TwinCAT.Ads;

namespace OC.Assistant.Core;

/// <summary>
/// Singleton class for the project state.
/// </summary>
public class ProjectState : ProjectStateView, IProjectStateEvents, IProjectStateSolution
{
    private static readonly Lazy<ProjectState> LazyInstance = new(() => new ProjectState());
    private readonly AdsClient _adsClient = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private AdsState _lastRunState = AdsState.Idle;
    private AmsNetId _amsNetId = AmsNetId.Local;
    private DTE? _dte;
    private ITcSysManager15? _tcSysManager;
    private bool _adsNotOk;
    private bool IsProjectConnected => FullName is not null;
    
    /// <summary>
    /// Gets the <see cref="ProjectStateView"/> element.
    /// </summary>
    public static ProjectStateView View => LazyInstance.Value;
    
    /// <summary>
    /// Gets the <see cref="IProjectStateSolution"/> interface.
    /// </summary>
    public static IProjectStateSolution Solution => LazyInstance.Value;
    
    /// <summary>
    /// Gets the <see cref="IProjectStateEvents"/> interface.
    /// </summary>
    public static IProjectStateEvents Events => LazyInstance.Value;
    
    public event Action<string>? Connected;
    public event Action? Disconnected;
    public event Action? StartedRunning;
    public event Action? StoppedRunning;
    public event Action<bool>? Locked;
    public string? FullName { get; private set; }
    
    private ProjectState()
    {
        if (LazyInstance.IsValueCreated) return;
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
    
    public void Connect(string path)
    {
        Dispatcher.Invoke(() =>
        {
            if (_adsNotOk || TcDte.GetInstance(path) is not {} dte) return;
            if (dte.GetProjectFolder() is not {} projectFolder) return;
            if (IsProjectConnected) Disconnect();
            
            _dte = dte;
            FullName = path;
        
            _tcSysManager = _dte.GetTcSysManager();
            _amsNetId = GetCurrentNetId();
            ApiLocal.Interface.NetId = _amsNetId;
            _cancellationTokenSource = new CancellationTokenSource();
            SetSolutionPath(path);
            StartPolling(UpdateNetId, 1000);
            StartPolling(UpdateAdsState, 100);
            XmlFile.Instance.SetDirectory(projectFolder);
            Connected?.Invoke(path);
            Logger.LogInfo(this, $"{path} connected");
        });
    }
    
    private void Disconnect()
    {
        Dispatcher.Invoke(() =>
        {
            Logger.LogWarning(this, "TwinCAT Project closed");
            FullName = null;
            Disconnected?.Invoke();
            Locked?.Invoke(true);
            _lastRunState = AdsState.Idle;
            _cancellationTokenSource.Cancel();
            _adsClient.Disconnect();
            _dte?.Finalize();
            IndicateDisconnected();
        });
    }

    private AdsState GetAdsState()
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
            Logger.LogError(this, e.Message);
            return AdsState.Exception;
        }
    }

    private AmsNetId GetCurrentNetId()
    {
        try
        {
            var netId = _tcSysManager?.GetTargetNetId();
            return netId is null ? _amsNetId : new AmsNetId(netId);
        }
        catch (Exception e)
        {
            if ((uint)e.HResult != 0x8001010A) //Server is busy; try later
            {
                Disconnect();
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
                    // Expected during cancellation, no action needed
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
        if (BusyState.IsSet || !IsProjectConnected) return Task.CompletedTask;
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
                Dispatcher.Invoke(() =>
                {
                    StartedRunning?.Invoke();
                    Locked?.Invoke(true);
                });
                
                break;
            default:
                IndicateConfigMode();
                Dispatcher.Invoke(() =>
                {
                    StoppedRunning?.Invoke();
                    Locked?.Invoke(false);
                });
                break;
        }
        
        return Task.CompletedTask;
    }
}