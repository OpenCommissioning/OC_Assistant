using System.Collections.Concurrent;
using OC.Assistant.Sdk;
using OC.Assistant.Sdk.Plugin;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat;

/// <summary>
/// Custom AdsServer for acyclic data communication.
/// </summary>
public class TcRecordDataServer() : TwinCAT.Ads.Server.AdsServer("Open Commissioning AdsServer"), IRecordDataServer
{
    private static readonly Lazy<TcRecordDataServer> Lazy = new(() => new TcRecordDataServer());
    private AmsAddress _amsAddress = AmsAddress.Empty;
    private readonly ConcurrentQueue<RecordDataTelegram> _writeReq = new();
    private readonly ConcurrentQueue<RecordDataTelegram> _readReq = new();
    private readonly Task<AdsErrorCode> _adsNoError = Task.FromResult(AdsErrorCode.NoError);
    
    public static TcRecordDataServer Instance => Lazy.Value;

    /// <inheritdoc cref="TwinCAT.Ads.Server.AdsServer.ConnectServer"/>
    public void Connect()
    {
        if (IsConnected) return;
        _amsAddress = new AmsAddress(TcState.Singleton.AmsNetId, TcState.Singleton.PlcPort);
        base.ConnectServer();
        Task.Run(MainCycle);
    }

    /// <inheritdoc cref="TwinCAT.Ads.Server.AdsServer.Disconnect"/>
    public new void Disconnect()
    {
        base.Disconnect();
    }
    
    private void MainCycle()
    {
        var stopwatch = new StopwatchEx();
                
        while (IsConnected)
        {
            stopwatch.WaitUntil(10);
                
            try
            {
                if (_writeReq.TryDequeue(out var recordData))
                {
                    var invokeId = recordData.Index | (uint)(recordData.HardwareId << 16);
                    var indexGroup = 0x80000000 + recordData.Index;
                    var indexOffset = recordData.HardwareId | (uint)(recordData.Identifier << 16);
                    
                    WriteRequest(
                        _amsAddress, 
                        invokeId, 
                        indexGroup, 
                        indexOffset, 
                        new ReadOnlySpan<byte>(recordData.Data)[..(int)recordData.CbLength]);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message, true);
            }
                
            try
            {
                if (_readReq.TryDequeue(out var recordData))
                {
                    var invokeId = recordData.Index | (uint)(recordData.HardwareId << 16);
                    var indexGroup = 0x80000000 + recordData.Index;
                    var indexOffset = recordData.HardwareId | (uint)(recordData.Identifier << 16);
                    
                    ReadRequest(
                        _amsAddress, 
                        invokeId, 
                        indexGroup, 
                        indexOffset, 
                        (int)recordData.CbLength);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(this, e.Message, true);
            }
        }
    }

    /// <summary>
    /// Sends a write-request using <see cref="TwinCAT.Ads.Server.AdsServer.WriteRequest"/> 
    /// </summary>
    public void WriteReq(RecordDataTelegram request)
    {
        var indexOffset = request.HardwareId | (uint)(request.Identifier << 16);
        if (RecordDataList.Contains(indexOffset))
        {
            _writeReq.Enqueue(request);
        }
    }
    
    /// <summary>
    /// Sends a read-request using <see cref="TwinCAT.Ads.Server.AdsServer.ReadRequest"/> 
    /// </summary>
    public void ReadReq(RecordDataTelegram request)
    {
        var indexOffset = request.HardwareId | (uint)(request.Identifier << 16);
        if (RecordDataList.Contains(indexOffset))
        {
            _readReq.Enqueue(request);       
        }
    }
    
    /// <inheritdoc />
    protected override Task<AdsErrorCode> OnWriteConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        var identifier = (ushort) result;
        var hardwareId = (ushort) (invokeId >> 16);
        var index = (ushort) invokeId;
        OnWriteRes?.Invoke(new RecordDataTelegram(identifier, hardwareId, index, 0));
        return _adsNoError;
    }

    /// <inheritdoc />
    protected override Task<AdsErrorCode> OnReadConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> data, CancellationToken cancel)
    {
        cancel.ThrowIfCancellationRequested();
        var identifier = (ushort) result;
        var hardwareId = (ushort) (invokeId >> 16);
        var index = (ushort) invokeId;
        OnReadRes?.Invoke(new RecordDataTelegram(identifier, hardwareId, index, (uint)data.Length, data.ToArray()));
        return _adsNoError;
    }
    
    /// <summary>
    /// Is raised when a <see cref="TwinCAT.Ads.Server.AdsServer.OnReadConfirmationAsync"/> has been received.
    /// </summary>
    public event Action<RecordDataTelegram>? OnReadRes;
    
    /// <summary>
    /// Is raised when a <see cref="TwinCAT.Ads.Server.AdsServer.OnWriteConfirmationAsync"/> has been received.
    /// </summary>
    public event Action<RecordDataTelegram>? OnWriteRes;
}