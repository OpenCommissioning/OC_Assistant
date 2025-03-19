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
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DteSingleThread.Run(() =>
        {
            DTE? dte = null;
            try
            {
                Logger.LogInfo(this, "Checking TwinCAT ADS server...");
                _adsClient.Connect((int) AmsPort.R0_Realtime);
                _adsClient.Disconnect();
                Logger.LogInfo(this, "TwinCAT ADS server ok");

                if (Environment.GetCommandLineArgs()
                        .FirstOrDefault(arg => arg.EndsWith(".sln")) is not {} solution) return;
                dte = TcDte.GetInstance(solution);
                if (dte?.GetProjectFolder() is not {} projectFolder) return;
                Connect(solution, projectFolder);
            }
            catch (Exception exception)
            {
                _adsNotOk = true;
                Logger.LogError(this, exception.Message);
            }
            finally
            {
                dte?.Finalize();
            }
        });
    }
    
    public void Connect(string solutionFullName, string projectFolder)
    {
        Dispatcher.Invoke(() =>
        {
            if (_adsNotOk) return;
            if (IsProjectConnected) Disconnect();
            FullName = solutionFullName;
            _cancellationTokenSource = new CancellationTokenSource();
            _amsNetId = GetCurrentNetId();
            ApiLocal.Interface.NetId = _amsNetId;
            XmlFile.Instance.SetDirectory(projectFolder);
            StartPolling(UpdateNetId, 1000);
            StartPolling(UpdateAdsState, 10);
            SetSolutionPath(solutionFullName);
            Connected?.Invoke(solutionFullName);
            Logger.LogInfo(this, $"{FullName} connected");
        });
    }
    
    private void Disconnect()
    {
        Dispatcher.Invoke(() =>
        {
            Logger.LogWarning(this, $"{FullName} disconnected");
            FullName = null;
            Disconnected?.Invoke();
            Locked?.Invoke(true);
            _lastRunState = AdsState.Idle;
            _cancellationTokenSource.Cancel();
            _adsClient.Disconnect();
            _dte?.Finalize();
            _tcSysManager = null;
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
            if (_tcSysManager is null && FullName is not null)
            {
                _dte = TcDte.GetInstance(FullName);
                _tcSysManager = _dte.GetTcSysManager();
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