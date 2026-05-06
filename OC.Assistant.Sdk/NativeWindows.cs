using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OC.Assistant.Sdk;

internal static class NativeWindows
{
    /// <summary>
    /// Provides a high-resolution waitable timer based on Windows native APIs.
    /// This timer allows precise waiting capabilities using manual reset behavior.
    /// </summary>
    public class WaitableTimer
    {
        private readonly IntPtr _timer;
        private readonly WaitHandle _waitHandle;

        /// <summary>
        /// Represents a high-resolution waitable timer implemented using Windows native APIs.
        /// Provides precise waiting capabilities and manual reset functionality.
        /// </summary>
        public WaitableTimer()
        {
            _timer = CreateWaitableTimerEx(
                IntPtr.Zero,
                null,
                CREATE_WAITABLE_TIMER_MANUAL_RESET | CREATE_WAITABLE_TIMER_HIGH_RESOLUTION,
                TIMER_ALL_ACCESS
            );
        
            _waitHandle = new AutoResetEvent(false)
            {
                SafeWaitHandle = new SafeWaitHandle(_timer, false)
            };
        }

        /// <summary>
        /// Waits for the specified time duration using a high-resolution waitable timer.
        /// </summary>
        /// <param name="dueTime">
        /// The time to wait, in 100-nanosecond intervals. Negative values indicate a relative time.
        /// </param>
        public void Wait(long dueTime)
        {
            if (dueTime >= 0 || !SetWaitableTimer(_timer, ref dueTime,
                    0, IntPtr.Zero, IntPtr.Zero, false))
            {
                return;
            }

            _waitHandle.WaitOne();
        }

        /// <summary>
        /// Closes the high-resolution waitable timer and releases all associated resources.
        /// This method ensures that native handles and resources are properly freed to prevent resource leaks.
        /// </summary>
        public void Close()
        {
            _waitHandle.Dispose();
            CloseHandle(_timer);
        }
    }
    
    private const uint TIMER_ALL_ACCESS = 0x001F0003;
    private const uint CREATE_WAITABLE_TIMER_MANUAL_RESET = 0x00000001;
    private const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;
    
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
        IntPtr hTimer,
        [In] ref long pDueTime,
        int lPeriod,
        IntPtr pfnCompletionRoutine,
        IntPtr lpArgToCompletionRoutine,
        bool fResume);
    
    /// <summary>
    /// See Win32 API documentation for
    /// <a href="https://learn.microsoft.com/de-de/windows/win32/api/handleapi/nf-handleapi-closehandle">
    /// CloseHandle</a>
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);
}