using EnvDTE;
using OC.Assistant.Sdk;
using TCatSysManagerLib;

namespace OC.Assistant.Twincat;

/// <summary>
/// <see cref="DTE"/> single-threaded invoker.
/// </summary>
public static class DteSingleThread
{
    /// <inheritdoc cref="Run(System.Action,int,bool)"/><br/>
    /// This overload automatically gets the <see cref="EnvDTE.DTE"/> interface of the currently connected solution.
    public static System.Threading.Thread Run(Action<ITcSysManager15> action, int millisecondsTimeout = 0, bool throwExceptions = false)
    {
        return Run(() =>
        {
            try
            {
                if (TcState.Singleton.SolutionFullName is null)
                {
                    throw new InvalidOperationException("Can't use TwinCAT interface. No solution connected.");
                }
                
                var tcSysManager = TcDte.GetTcSysManager(TcState.Singleton.SolutionFullName);
                TcDte.TrackObject(tcSysManager);
                if (tcSysManager is null) return;
                action(tcSysManager);
            }
            finally
            {
                TcDte.ReleaseTrackedObjects();
            }
        }, millisecondsTimeout, throwExceptions);
    }

    /// <summary>
    /// Registers a <see cref="MessageFilter"/> and invokes the given delegate in a new <see cref="System.Threading.Thread"/>
    /// with <see cref="ApartmentState"/> <see cref="ApartmentState.STA"/>.<br/>
    /// Is used to invoke COM functions with <see cref="EnvDTE.DTE"/> interface.  
    /// </summary>
    /// <param name="action">The action to be invoked in single-threaded apartment.</param>
    /// <param name="millisecondsTimeout">Blocks the calling thread until this thread terminates or the timeout is reached.
    /// Value 0 disables blocking and activates the busy state.
    /// </param>
    /// <param name="throwExceptions">Throw exceptions if true.</param>
    /// <returns>The instance of this <see cref="System.Threading.Thread"/>.</returns>
    public static System.Threading.Thread Run(Action action, int millisecondsTimeout = 0, bool throwExceptions = false)
    {
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                if (millisecondsTimeout == 0) BusyState.Set(action);
                MessageFilter.Register();
                action();
            }
            catch (Exception e)
            {
                Logger.LogError(typeof(DteSingleThread), e.Message);
                if (throwExceptions) throw;
            }
            finally
            {
                MessageFilter.Revoke();
                if (millisecondsTimeout == 0) BusyState.Reset(action);
            }
        });

#pragma warning disable CA1416
        thread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416
        thread.Start();
        if (millisecondsTimeout > 0) thread.Join(millisecondsTimeout);
        return thread;
    }
}