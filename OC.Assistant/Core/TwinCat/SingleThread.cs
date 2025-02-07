namespace OC.Assistant.Core.TwinCat;

/// <summary>
/// Generic single-threaded invoker.
/// </summary>
public static class SingleThread
{
    /// <summary>
    /// Registers a <see cref="MessageFilter"/> and invokes the given delegate in a new <see cref="Thread"/>
    /// with <see cref="ApartmentState"/> <see cref="ApartmentState.STA"/>.<br/>
    /// Can be used to invoke COM functions like the <see cref="EnvDTE.DTE"/> interface.  
    /// </summary>
    /// <param name="action">The action to be invoked in single-threaded apartment.</param>
    public static void Run(Action action)
    {
        var thread = new Thread(() =>
        {
            MessageFilter.Register();
            action();
            MessageFilter.Revoke();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }
}