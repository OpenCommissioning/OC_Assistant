using System.Collections.Concurrent;
using OC.Assistant.Sdk;
using TwinCAT.Ads;

namespace OC.Assistant.Twincat.Plugins.RecordData;

internal class PnAdsServer : TwinCAT.Ads.Server.AdsServer
{
    private readonly AmsAddress _plcAddress;
    private readonly ConcurrentQueue<Telegram> _writeInd = new();
    private readonly ConcurrentQueue<Telegram> _readInd = new();
    private readonly ConcurrentDictionary<uint, (AmsAddress, uint)> _writeRes = new();
    private readonly ConcurrentDictionary<uint, (AmsAddress, uint)> _readRes = new();
        
    /// <summary>
    /// Custom <see cref="TwinCAT.Ads.Server.AdsServer"/> to catch and forward read- and write-requests.
    /// </summary>
    public PnAdsServer(ushort port) : base(port, "Open Commissioning AdsServer for Profinet")
    {
        _plcAddress = new AmsAddress(TcState.Singleton.AmsNetId, TcState.Singleton.PlcPort);
        base.ConnectServer();
        Task.Run(Update);
        Logger.LogInfo(this, $"AdsServer {AmsServer.ServerAddress?.NetId}:{AmsServer.ServerAddress?.Port} connected and started");
    }

    /// <summary>
    /// Catches write indication and stores to fifo.
    /// <inheritdoc/>
    /// </summary>
    /// <inheritdoc/>
    protected override Task<ResultWrite> OnWriteAsync(AmsAddress amsAddress, uint invokeId, uint indexGroup, uint indexOffset, ReadOnlyMemory<byte> data, CancellationToken cancel)
    {
        var iGrp = indexGroup & 0x8000ffff;
        var telegram = new Telegram(amsAddress, invokeId, iGrp, indexOffset, (uint)data.Length, data.ToArray());
        
        if (!RecordDataList.Contains(telegram.Key))
        {
            return Task.FromResult(ResultWrite.CreateError(AdsErrorCode.DeviceServiceNotSupported, invokeId));
        }
        
        _writeInd.Enqueue(telegram);
        Logger.LogInfo(this, $"AdsWriteInd from {amsAddress.NetId}:{amsAddress.Port} IGrp {iGrp:X} IOffs {indexOffset:X}", true);
        return Task.FromResult(ResultWrite.CreateError(AdsErrorCode.NoError, invokeId));
    }
        
    /// <summary>
    /// Catches read indication and stores to fifo.
    /// <inheritdoc />
    /// </summary>
    /// <inheritdoc />
    protected override Task<ResultReadBytes> OnReadAsync(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, int cbLength, CancellationToken cancel)
    {
        var iGrp = indexGroup & 0x8000ffff;
        var telegram = new Telegram(rAddr, invokeId, iGrp, indexOffset, (uint)cbLength, null);
        
        if (!RecordDataList.Contains(telegram.Key))
        {
            return Task.FromResult(ResultReadBytes.CreateError(AdsErrorCode.DeviceServiceNotSupported, invokeId));
        }
        
        _readInd.Enqueue(telegram);
        Logger.LogInfo(this, $"AdsReadInd from {rAddr.NetId}:{rAddr.Port} IGrp {iGrp:X} IOffs {indexOffset:X}", true);
        return Task.FromResult(ResultReadBytes.CreateError(AdsErrorCode.NoError, invokeId));
    }
    
    /// <summary>
    /// Catches a write-response, removes the matched telegram from the dictionary, and forwards to origin.
    /// <inheritdoc/>
    /// </summary>
    /// <inheritdoc/>
    protected override async Task<AdsErrorCode> OnWriteConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, CancellationToken cancel)
    {
        // Check if the appropriate telegram has been stored
        if (!_writeRes.TryRemove(invokeId, out var target))
        {
            return AdsErrorCode.DeviceServiceNotSupported;
        }
        
        // Send response to origin
        await WriteResponseAsync(target.Item1, target.Item2, result, cancel);
        Logger.LogInfo(this, $"AdsWriteRes to {target.Item1.NetId}:{target.Item1.Port}", true);
        return AdsErrorCode.NoError;
    }
    
    
    /// <summary>
    /// Catches a read-response, removes the matched telegram from the dictionary, and forwards to origin.
    /// <inheritdoc/>
    /// </summary>
    /// <inheritdoc/>
    protected override async Task<AdsErrorCode> OnReadConfirmationAsync(AmsAddress rAddr, uint invokeId, AdsErrorCode result, ReadOnlyMemory<byte> data, CancellationToken cancel)
    {
        // Check if the appropriate telegram has been stored
        if (!_readRes.TryRemove(invokeId, out var target))
        {
            return AdsErrorCode.DeviceServiceNotSupported;
        }

        // Send response to origin
        await ReadResponseAsync(target.Item1, target.Item2, result, data, cancel);
        Logger.LogInfo(this, $"AdsReadRes to {target.Item1.NetId}:{target.Item1.Port}", true);
        return AdsErrorCode.NoError;
    }

    /// <summary>
    /// Sends stored read- and write-requests to the Plc.
    /// </summary>
    private void Update()
    {
        var stopwatch = new StopwatchEx();
                
        while (IsConnected)
        {
            stopwatch.WaitUntil(10);
                
            try
            {
                if (_writeInd.TryDequeue(out var telegram))
                {
                    var key = telegram.Key;
                        
                    // Store origin to the write result dictionary
                    _writeRes.TryAdd(key, (telegram.AmsAddress, telegram.InvokeId));
                        
                    // Send the telegram to the Plc, use key as invokeId
                    WriteRequest(
                        _plcAddress, key,
                        telegram.IndexGroup,
                        telegram.IndexOffset,
                        new ReadOnlySpan<byte>(telegram.Data, 0, (int)telegram.Length));
                }
            }
            catch(Exception e)
            {
                Logger.LogError(this, $"AdsServer {AmsServer.ServerAddress?.NetId}:{AmsServer.ServerAddress?.Port} error: {e.Message}", true);
            }
                
            try
            {
                if (_readInd.TryDequeue(out var telegram))
                {
                    var key = telegram.Key;
                        
                    // Store origin to the read result dictionary
                    _readRes.TryAdd(key, (telegram.AmsAddress, telegram.InvokeId));
                        
                    // Send the telegram to the Plc, use key as invokeId
                    ReadRequest(
                        _plcAddress, 
                        key, 
                        telegram.IndexGroup, 
                        telegram.IndexOffset,
                        (int)telegram.Length);
                }
            }
            catch(Exception e)
            {
                Logger.LogError(this, $"AdsServer {AmsServer.ServerAddress?.NetId}:{AmsServer.ServerAddress?.Port} error: {e.Message}", true);
            }
        }
            
        Logger.LogInfo(this, $"AdsServer {AmsServer.ServerAddress?.NetId}:{AmsServer.ServerAddress?.Port} disconnected and stopped");
    }
}