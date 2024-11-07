using System.Diagnostics;
using OC.Assistant.Sdk;

namespace OC.Assistant.Core;

/// <summary>
/// Generic retry invoker.
/// </summary>
public static class Retry
{
    /// <summary>
    /// Invokes the given <see cref="Func{TResult}"/> until success or timeout.
    /// </summary>
    /// <typeparam name="TResult">The return type of the invoked func.</typeparam>
    /// <param name="func">The func to be invoked.</param>
    /// <param name="millisecondsTimeout">Retry duration.</param>
    /// <returns>The return value of the invoked func.</returns>
    /// <exception cref="Exception">The latest exception from the invoked func when the timeout is reached.</exception>
    public static TResult? Invoke<TResult>(Func<TResult?> func, int millisecondsTimeout = 2000)
    {
        var exception = new Exception();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        while (stopwatch.ElapsedMilliseconds < millisecondsTimeout)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                exception = e;
                Logger.LogError(typeof(Retry), e.Message, true);
                Thread.Sleep(100);
            }
        }

        if (exception is System.Runtime.InteropServices.COMException)
        {
            ComException.Raise();
        }
        throw exception;
    }
    
    /// <summary>
    /// Invokes the given <see cref="Action"/> until success or timeout.
    /// </summary>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="millisecondsTimeout">Retry duration.</param>
    /// <exception cref="Exception">The latest exception from the invoked action when the timeout is reached.</exception>
    public static void Invoke(Action action, int millisecondsTimeout = 2000)
    {
        var exception = new Exception();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        while (stopwatch.ElapsedMilliseconds < millisecondsTimeout)
        {
            try
            {
                action();
                return;
            }
            catch (Exception e)
            {
                exception = e;
                Logger.LogError(typeof(Retry), e.Message, true);
                Thread.Sleep(100);
            }
        }

        if (exception is System.Runtime.InteropServices.COMException)
        {
            ComException.Raise();
        }
        throw exception;
    }
}