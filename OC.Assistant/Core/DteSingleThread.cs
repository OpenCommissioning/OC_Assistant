using EnvDTE;

namespace OC.Assistant.Core;

/// <summary>
/// <see cref="DTE"/> single-threaded invoker.
/// </summary>
public static class DteSingleThread
{
    /// <summary>
    /// Registers a <see cref="MessageFilter"/> and invokes the given delegate in a new <see cref="System.Threading.Thread"/>
    /// with <see cref="ApartmentState"/> <see cref="ApartmentState.STA"/>.<br/>
    /// Is used to invoke COM functions with <see cref="EnvDTE.DTE"/> interface.  
    /// </summary>
    /// <param name="action">The action to be invoked in single-threaded apartment.</param>
    public static void Run(Action<DTE> action)
    {
        var thread = new System.Threading.Thread(() =>
        {
            BusyState.Set(action);
            MessageFilter.Register();
            if (TcDte.GetInstance(ProjectState.Solution.FullName) is {} dte)
            {
                action(dte);
                dte.Finalize();
            }
            MessageFilter.Revoke();
            BusyState.Reset(action);
        });
        
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }
}