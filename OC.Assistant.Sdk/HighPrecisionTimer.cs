using System.Diagnostics;

namespace OC.Assistant.Sdk;

internal interface IHighPrecisionTimer : IDisposable
{
    void Reset();
    void WaitUntil(long millisecondsTimeout);
}

internal static class HighPrecisionTimer
{
    public static IHighPrecisionTimer Create(Stopwatch stopwatch)
    {
        if (OperatingSystem.IsWindows()) return new WindowsTimer(stopwatch);
        if (OperatingSystem.IsLinux()) return new LinuxTimer();
        if (OperatingSystem.IsMacOS()) return new MacOsTimer();
        throw new PlatformNotSupportedException();
    }
}

internal sealed class WindowsTimer(Stopwatch stopwatch) : IHighPrecisionTimer
{
    private readonly NativeWindows.WaitableTimer _timer = new();

    public void Reset() { }

    public void WaitUntil(long millisecondsTimeout)
    {
        _timer.Wait(stopwatch.Elapsed.Ticks - millisecondsTimeout * 10_000L);
    }

    public void Dispose() => _timer.Dispose();
}

internal sealed class LinuxTimer : IHighPrecisionTimer
{
    private long _startTimeNs;

    public void Reset() => _startTimeNs = NativeLinux.GetMonotonicTimeNanoseconds();

    public void WaitUntil(long millisecondsTimeout)
    {
        NativeLinux.SleepUntil(_startTimeNs + millisecondsTimeout * 1_000_000L);
        Reset();
    }

    public void Dispose() { }
}

internal sealed class MacOsTimer : IHighPrecisionTimer
{
    private long _startTimeNs;

    public void Reset() => _startTimeNs = NativeMacOs.GetMonotonicTimeNanoseconds();

    public void WaitUntil(long millisecondsTimeout)
    {
        NativeMacOs.SleepUntil(_startTimeNs + millisecondsTimeout * 1_000_000L);
        Reset();
    }

    public void Dispose() { }
}