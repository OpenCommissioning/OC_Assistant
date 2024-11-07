using System.Diagnostics;

namespace OC.Assistant.Sdk;

/// <summary>
/// <see cref="System.Diagnostics.Stopwatch"/> extension methods.
/// </summary>
public static class StopwatchExtension
{
    /// <summary>
    /// Sleeps for the amount of milliseconds, calculated by the given timeout minus the already elapsed time 
    /// and restarts the <see cref="Stopwatch"/>.
    /// <param name="stopwatch">The <see cref="System.Diagnostics.Stopwatch"/> to be extended.</param>
    /// <param name="millisecondsTimeout">The number of milliseconds.</param>
    /// </summary>
    public static void WaitUntil(this Stopwatch stopwatch, int millisecondsTimeout)
    {
        var delta = millisecondsTimeout - (int) stopwatch.Elapsed.TotalMilliseconds;
        if (delta > 0) Thread.Sleep(delta);
        stopwatch.Restart();
    }
}