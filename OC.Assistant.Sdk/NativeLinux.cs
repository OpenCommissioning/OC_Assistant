using System.Runtime.InteropServices;

namespace OC.Assistant.Sdk;

internal static class NativeLinux
{
    private const int CLOCK_MONOTONIC = 1;
    private const int TIMER_ABSOLUTE_TIME = 1;
    private const int EINTR = 4;
    
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8981
    // ReSharper disable once InconsistentNaming
    private struct timespec
#pragma warning restore CS8981
    {
        public long tv_sec;
        public long tv_nsec;
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int clock_nanosleep(
        int clockId,
        int flags,
        ref timespec request,
        IntPtr remain);
    
    [DllImport("libc", SetLastError = true)]
    private static extern int clock_gettime(
        int clockId,
        out timespec tp);
    
    public static long GetMonotonicTimeNanoseconds()
    {
        if (clock_gettime(CLOCK_MONOTONIC, out var ts) != 0)
        {
            throw new InvalidOperationException($"clock_gettime failed with errno {Marshal.GetLastWin32Error()}");
        }
        return ts.tv_sec * 1_000_000_000L + ts.tv_nsec;
    }

    public static void SleepUntil(long targetNanoseconds)
    {
        var ts = new timespec
        {
            tv_sec = targetNanoseconds / 1_000_000_000,
            tv_nsec = targetNanoseconds % 1_000_000_000
        };

        // clock_nanosleep returns the errno value directly (not via Marshal.GetLastWin32Error),
        // and may return EINTR if interrupted by a signal — retry against the same absolute deadline.
        int result;
        do
        {
            result = clock_nanosleep(CLOCK_MONOTONIC, TIMER_ABSOLUTE_TIME, ref ts, IntPtr.Zero);
        }
        while (result == EINTR);
    }
}