using System.Diagnostics;

namespace OC.Assistant.Sdk;

internal interface IHighPrecisionTimer : IDisposable
{
    Stopwatch Clock { get; }
    void Restart();
    void WaitUntil(long millisecondsTimeout);
}

internal static class HighPrecisionTimer
{
    public static IHighPrecisionTimer Create()
    {
        if (OperatingSystem.IsWindows()) return new WindowsTimer();
        if (OperatingSystem.IsLinux()) return new LinuxTimer();
        if (OperatingSystem.IsMacOS()) return new MacOsTimer();
        throw new PlatformNotSupportedException();
    }
}

internal sealed class WindowsTimer : IHighPrecisionTimer
{
    private readonly NativeWindows.WaitableTimer _timer = new();

    public Stopwatch Clock { get; } = new();

    public void Restart() => Clock.Restart();

    public void WaitUntil(long millisecondsTimeout)
    {
        _timer.Wait(Clock.Elapsed.Ticks - millisecondsTimeout * 10_000L);
        Restart();
    }

    public void Dispose() => _timer.Dispose();
}

internal sealed class LinuxTimer : IHighPrecisionTimer
{
    private long _startTimeNs;
    
    public Stopwatch Clock { get; } = new();

    public void Restart()
    {
        _startTimeNs = NativeLinux.GetMonotonicTimeNanoseconds();
        Clock.Restart();
    }

    public void WaitUntil(long millisecondsTimeout)
    {
        NativeLinux.SleepUntil(_startTimeNs + millisecondsTimeout * 1_000_000L);
        Restart();
    }

    public void Dispose() { }
}

internal sealed class MacOsTimer : IHighPrecisionTimer
{
    private long _startTimeNs;
    
    public Stopwatch Clock { get; } = new();

    public void Restart()
    {
        _startTimeNs = NativeMacOs.GetMonotonicTimeNanoseconds();
        Clock.Restart();
    }

    public void WaitUntil(long millisecondsTimeout)
    {
        NativeMacOs.SleepUntil(_startTimeNs + millisecondsTimeout * 1_000_000L);
        Restart();
    }

    public void Dispose() { }
}