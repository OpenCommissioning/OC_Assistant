using System.Windows;
using System.Xml.Linq;
using OC.Assistant.Core;
using OC.Assistant.Sdk;
using TCatSysManagerLib;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat;

public class TcState
{
    private static readonly Lazy<TcState> Lazy = new(() => new TcState());
    
    private readonly AdsClient _adsClient = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private AdsState _adsState = AdsState.Idle;
    private AmsNetId _amsNetId = AmsNetId.Local;
    private ITcSysManager15? _tcSysManager;
    private bool IsProjectConnected => SolutionFullName is not null;
    
    public static TcState Instance => Lazy.Value;

    private TcState()
    {
        if (Lazy.IsValueCreated) return;
        Task.Run(() =>
        {
            try
            {
                _adsClient.Connect((int) AmsPort.R0_Realtime);
                _adsClient.Disconnect();
                ProjectState.Events.Connected += Connect;
                ProjectState.Events.Disconnected += Disconnect;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Validated?.Invoke();
                });
                return true;
            }
            catch
            {
                return false;
            }
        });
    }
    
    public string? SolutionFullName { get; private set; }
    public event Action? Validated;
    
    private void Connect(string solutionFullName)
    {
        SolutionFullName = solutionFullName;
        _cancellationTokenSource = new CancellationTokenSource();
        _amsNetId = GetCurrentNetId();
        ApiLocal.Interface.NetId = _amsNetId;
        StartPolling(UpdateNetId, 1000);
        StartPolling(UpdateAdsState, 10);
    }
    
    private void Disconnect()
    {
        SolutionFullName = null;
        _cancellationTokenSource.Cancel();
        _adsClient.Disconnect();
        ComHelper.ReleaseObject(_tcSysManager);
        _tcSysManager = null;
        _adsState = AdsState.Idle;
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
                ProjectState.Control.Stop();
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
                ProjectState.Control.Disconnect();
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
        if (BusyState.IsSet || !IsProjectConnected) return Task.CompletedTask;
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

    private Task UpdateAdsState()
    {
        if (!IsProjectConnected) return Task.CompletedTask;
        
        var adsState = GetAdsState();
        if (adsState == _adsState) return Task.CompletedTask;
        _adsState = adsState;

        switch (adsState)
        {
            case AdsState.Run:
                ApiLocal.Interface.Port = GetPlcPort();
                ApiLocal.Interface.TriggerTcRestart();
                ProjectState.Control.Start();
                break;
            default:
                ProjectState.Control.Stop();
                break;
        }
        
        return Task.CompletedTask;
    }
}