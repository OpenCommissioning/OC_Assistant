using System.Runtime.InteropServices;

namespace OC.Assistant;

public abstract class WinApi
{
    /// <summary>
    /// timeBeginPeriod function<br/>
    /// See Windows API documentation for details.
    /// </summary>
    [DllImport("winmm.dll", EntryPoint="timeBeginPeriod", SetLastError=true)]
    public static extern uint TimeBeginPeriod(uint uMilliseconds);

    /// <summary>
    /// timeEndPeriod function<br/>
    /// See Windows API documentation for details.
    /// </summary>
    [DllImport("winmm.dll", EntryPoint="timeEndPeriod", SetLastError=true)]
    public static extern uint TimeEndPeriod(uint uMilliseconds);
}