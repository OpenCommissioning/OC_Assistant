using EnvDTE;

namespace OC.Assistant.Core;

/// <summary>
/// <see cref="DTE"/> single-threaded invoker.
/// </summary>
public static class DteSingleThread
{
    /// <inheritdoc cref="Run(System.Action,int)"/><br/>
    /// This overload automatically gets the <see cref="EnvDTE.DTE"/> interface of the currently connected solution.
    public static System.Threading.Thread Run(Action<DTE> action, int millisecondsTimeout = 0)
    {
        return Run(() =>
        {
            if (ProjectState.Solution.FullName is null)
            {
                Sdk.Logger.LogError(typeof(DteSingleThread), "No Solution selected");
                return;
            }

            DTE? dte = null;

            try
            {
                dte = TcDte.GetInstance(ProjectState.Solution.FullName);
                if (dte is null) return;
                action(dte);
            }
            catch (Exception e)
            {
                Sdk.Logger.LogError(typeof(DteSingleThread), e.Message);
            }
            finally
            {
                dte?.Finalize();
            }
        }, millisecondsTimeout);
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
    /// <returns>The instance of this <see cref="System.Threading.Thread"/>.</returns>
    public static System.Threading.Thread Run(Action action, int millisecondsTimeout = 0)
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
                Sdk.Logger.LogError(typeof(DteSingleThread), e.Message);
            }
            finally
            {
                MessageFilter.Revoke();
                if (millisecondsTimeout == 0) BusyState.Reset(action);
            }
        });
        
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (millisecondsTimeout > 0) thread.Join(millisecondsTimeout);
        return thread;
    }
}