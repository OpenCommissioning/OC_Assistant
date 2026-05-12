using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OC.Assistant.Sdk;

internal static class NativeWindows
{
    /// <summary>
    /// Provides a high-resolution waitable timer based on Windows native APIs.
    /// </summary>
    public sealed class WaitableTimer : IDisposable
    {
        private readonly SafeWaitHandle _timerHandle = new(CreateTimerHandle(), ownsHandle: true);

        /// <summary>
        /// Waits for the specified time duration using a high-resolution waitable timer.
        /// </summary>
        /// <param name="dueTime">
        /// The time to wait, in 100-nanosecond intervals. Negative values indicate a relative time.
        /// Non-negative values return immediately without waiting.
        /// </param>
        public void Wait(long dueTime)
        {
            if (dueTime >= 0) return;

            if (!SetWaitableTimer(_timerHandle, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            if (WaitForSingleObject(_timerHandle, INFINITE) == WAIT_FAILED)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void Dispose()
        {
            _timerHandle.Dispose();
        }

        private static IntPtr CreateTimerHandle()
        {
            // CREATE_WAITABLE_TIMER_HIGH_RESOLUTION requires Windows 10 1803 or later.
            // Fall back to a regular waitable timer on older systems.
            var handle = CreateWaitableTimerEx(IntPtr.Zero, null,
                CREATE_WAITABLE_TIMER_MANUAL_RESET | CREATE_WAITABLE_TIMER_HIGH_RESOLUTION,
                TIMER_ALL_ACCESS);
            if (handle != IntPtr.Zero) return handle;

            handle = CreateWaitableTimerEx(IntPtr.Zero, null,
                CREATE_WAITABLE_TIMER_MANUAL_RESET, TIMER_ALL_ACCESS);
            if (handle != IntPtr.Zero) return handle;

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private const uint TIMER_ALL_ACCESS = 0x001F0003;
    private const uint CREATE_WAITABLE_TIMER_MANUAL_RESET = 0x00000001;
    private const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;
    private const uint INFINITE = 0xFFFFFFFF;
    private const uint WAIT_FAILED = 0xFFFFFFFF;

    /// <summary>
    /// See Win32 API documentation for
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-createwaitabletimerexw">
    /// CreateWaitableTimerEx</a>
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateWaitableTimerEx(
        IntPtr lpTimerAttributes,
        string? lpName,
        uint dwFlags,
        uint dwDesiredAccess);

    /// <summary>
    /// See Win32 API documentation for
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-setwaitabletimer">
    /// SetWaitableTimer</a>
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetWaitableTimer(
        SafeWaitHandle hTimer,
        [In] ref long pDueTime,
        int lPeriod,
        IntPtr pfnCompletionRoutine,
        IntPtr lpArgToCompletionRoutine,
        bool fResume);

    /// <summary>
    /// See Win32 API documentation for
    /// <a href="https://learn.microsoft.com/en-us/windows/win32/api/synchapi/nf-synchapi-waitforsingleobject">
    /// WaitForSingleObject</a>
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);
}
