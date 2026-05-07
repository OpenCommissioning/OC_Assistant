using System.Runtime.InteropServices;

namespace OC.Assistant.Sdk;

internal static class NativeMacOs
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8981
    // ReSharper disable once InconsistentNaming
    private struct mach_timebase_info_data_t
#pragma warning restore CS8981
    {
        public uint numer;
        public uint denom;
    }

    [DllImport("libSystem.dylib")]
    private static extern int mach_timebase_info(out mach_timebase_info_data_t info);

    [DllImport("libSystem.dylib")]
    private static extern ulong mach_absolute_time();

    [DllImport("libSystem.dylib")]
    private static extern int mach_wait_until(ulong deadline);

    private static readonly double NanosPerTick;
    private static readonly double TicksPerNano;

    static NativeMacOs()
    {
        _ = mach_timebase_info(out var tb);
        // mach_absolute_time returns "ticks"; convert with numer/denom to nanoseconds.
        NanosPerTick = (double)tb.numer / tb.denom;
        TicksPerNano = (double)tb.denom / tb.numer;
    }

    public static long GetMonotonicTimeNanoseconds()
    {
        var ticks = mach_absolute_time();
        return (long)(ticks * NanosPerTick);
    }

    public static void SleepUntil(long targetNanoseconds)
    {
        var deadlineTicks = (ulong)(targetNanoseconds * TicksPerNano);
        _ = mach_wait_until(deadlineTicks);
    }
}