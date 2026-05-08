using System.Diagnostics;

namespace OC.Assistant.Sdk;

/// <inheritdoc cref="Stopwatch"/>
/// <Remarks>
/// This extended version provides a high-precision <see cref="WaitUntil"/> method.
/// </Remarks>
public sealed class StopwatchEx : Stopwatch, IDisposable
{
    private readonly IHighPrecisionTimer _timer;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StopwatchEx"/> class.
    /// </summary>
    public StopwatchEx()
    {
        _timer = HighPrecisionTimer.Create(this);
    }
    
    /// <summary>
    /// Starts a new <see cref="StopwatchEx"/> instance and returns it.
    /// </summary>
    public new static StopwatchEx StartNew()
    {
        StopwatchEx s = new();
        s.Start();
        return s;
    }

    /// <inheritdoc cref="Stopwatch.Start"/>
    public new void Start()
    {
        _timer.Reset();
        base.Start();
    }

    /// <inheritdoc cref="Stopwatch.Restart"/>
    public new void Restart()
    {
        _timer.Reset();
        base.Restart();
    }

    /// <summary>
    /// Waits until the given timeout has elapsed since the
    /// last <see cref="Start"/> or <see cref="Restart"/>.
    /// Automatically restarts the <see cref="Stopwatch"/> after completion.
    /// </summary>
    /// <param name="millisecondsTimeout">The timeout in milliseconds.</param>
    public void WaitUntil(long millisecondsTimeout)
    {
        _timer.WaitUntil(millisecondsTimeout);
        base.Restart();
    }

    /// <summary>
    /// Releases all resources used by this <see cref="StopwatchEx"/> instance.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();
    }
}