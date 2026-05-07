using System.Diagnostics;

namespace OC.Assistant.Sdk;

/// <summary>
/// Provides an extended <see cref="Stopwatch"/>,
/// using a high-resolution timer that can be used to wait with millisecond precision.
/// </summary>
public class StopwatchEx : Stopwatch, IDisposable
{
    private enum Platform { Windows, Linux, MacOs }
    private readonly Platform _platform;
    private readonly NativeWindows.WaitableTimer? _waitableTimer;
    private long _startTimeStamp;
    
    private static Platform GetOperatingSystem()
    {
        Platform platform;
        if (OperatingSystem.IsWindows()) platform = Platform.Windows;
        else if (OperatingSystem.IsLinux()) platform = Platform.Linux;
        else if (OperatingSystem.IsMacOS()) platform = Platform.MacOs;
        else throw new NotSupportedException();
        return platform;
    }
    
    private static NativeWindows.WaitableTimer? GetWaitableTimer(Platform platform)
    {
        return platform switch
        {
            Platform.Windows => new NativeWindows.WaitableTimer(),
            _ => null
        };
    }

    private static long GetTimeStamp(Platform platform)
    {
        return platform switch
        {
            Platform.Linux => NativeLinux.GetMonotonicTimeNanoseconds(),
            Platform.MacOs => NativeMacOs.GetMonotonicTimeNanoseconds(),
            _ => 0
        };
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="StopwatchEx"/> class.
    /// </summary>
    public StopwatchEx()
    {
        _platform = GetOperatingSystem();
        _waitableTimer = GetWaitableTimer(_platform);
    }
    
    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="StopwatchEx"/> class.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _waitableTimer?.Close();
    }
    
    /// <inheritdoc cref="Stopwatch.Start"/>
    public new void Start()
    {
        _startTimeStamp = GetTimeStamp(_platform);
        base.Start();
    }
    
    /// <inheritdoc cref="Stopwatch.Restart"/>
    public new void Restart()
    {
        _startTimeStamp = GetTimeStamp(_platform);
        base.Restart();
    }

    /// <summary>
    /// Waits a number of milliseconds,
    /// calculated by the given timeout minus the already elapsed time and restarts the <see cref="Stopwatch"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds.</param>
    public void WaitUntil(long millisecondsTimeout)
    {
        switch (_platform)
        {
            case Platform.Windows:
                //dueTime in 100-nanosecond intervals, negative values indicate relative time
                _waitableTimer?.Wait(Elapsed.Ticks - millisecondsTimeout * 10_000);
                break;
            case Platform.Linux:
                NativeLinux.SleepUntil(_startTimeStamp + millisecondsTimeout * 1_000_000);
                break;
            case Platform.MacOs:
                NativeMacOs.SleepUntil(_startTimeStamp + millisecondsTimeout * 1_000_000);
                break;
        }
        
        Restart();
    }
}