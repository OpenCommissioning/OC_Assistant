using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OC.Assistant.Sdk;

/// <summary>
/// Provides an extended <see cref="Stopwatch"/>,
/// using a high resolution timer that can be used to wait with milliseconds precision.
/// </summary>
public class StopwatchEx : Stopwatch, IDisposable
{
    private readonly bool _isLinux;
    private readonly NativeWindows.WaitableTimer? _waitableTimer;
    private long _linuxStartTimeStamp;
    
    /// <summary>
    /// Creates a new instance of the <see cref="StopwatchEx"/> class.
    /// </summary>
    public StopwatchEx()
    {
        _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        if (_isLinux) return;

        _waitableTimer = new NativeWindows.WaitableTimer();
    }
    
    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="StopwatchEx"/> class.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isLinux) return;
        _waitableTimer?.Close();
    }
    
    /// <inheritdoc cref="Stopwatch.Start"/>
    public new void Start()
    {
        if (_isLinux) _linuxStartTimeStamp = NativeLinux.GetMonotonicTimeNanoseconds();
        base.Start();
    }
    
    /// <inheritdoc cref="Stopwatch.Restart"/>
    public new void Restart()
    {
        if (_isLinux) _linuxStartTimeStamp = NativeLinux.GetMonotonicTimeNanoseconds();
        base.Restart();
    }

    /// <summary>
    /// Waits a number of milliseconds,
    /// calculated by the given timeout minus the already elapsed time and restarts the <see cref="Stopwatch"/>.
    /// </summary>
    /// <param name="millisecondsTimeout">The number of milliseconds.</param>
    public void WaitUntil(long millisecondsTimeout)
    {
        if (_isLinux)
        {
            var target = _linuxStartTimeStamp + millisecondsTimeout * 1_000_000;
            NativeLinux.SleepUntil(target);
            Restart();
            return;
        }

        //dueTime in 100 nanosecond intervals, negative values indicate relative time
        var dueTime = ElapsedTicks - millisecondsTimeout * 10_000;
        _waitableTimer?.Wait(dueTime);
        Restart();
    }
}