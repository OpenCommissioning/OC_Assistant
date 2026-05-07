using System.Diagnostics;

namespace OC.Assistant.Sdk;

/// <summary>
/// Provides a multi-platform high-precision timer.
/// Not thread-safe - use one instance per thread.
/// </summary>
public sealed class StopwatchEx : IDisposable
{
    private readonly IHighPrecisionTimer _timer = HighPrecisionTimer.Create();
    
    /// <inheritdoc cref="Stopwatch.Elapsed"/>
    public TimeSpan Elapsed => _timer.Clock.Elapsed;

    /// <inheritdoc cref="Stopwatch.ElapsedMilliseconds"/>
    public long ElapsedMilliseconds => _timer.Clock.ElapsedMilliseconds;

    /// <inheritdoc cref="Stopwatch.Start"/>
    public void Start()
    {
        _timer.Restart();
    }

    /// <inheritdoc cref="Stopwatch.Restart"/>
    public void Restart()
    {
        _timer.Restart();
    }

    /// <summary>
    /// Waits until the given timeout has elapsed since the
    /// last <see cref="Start"/> or <see cref="Restart"/>.
    /// Automatically restarts the timer after completion.
    /// </summary>
    /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
    public void WaitUntil(long millisecondsTimeout)
    {
        _timer.WaitUntil(millisecondsTimeout);
    }

    /// <summary>
    /// Releases all resources used by this <see cref="StopwatchEx"/> instance.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();
    }
}