namespace OC.Assistant.Core;

/// <summary>
/// Static class that can be used to raise an event
/// in case of a <see cref="System.Runtime.InteropServices.COMException"/>.
/// The event is used to disconnect the connected solution.
/// </summary>
public static class ComException
{
    /// <summary>
    /// Raise the event in case of a <see cref="System.Runtime.InteropServices.COMException"/>.
    /// </summary>
    public static void Raise()
    {
        Raised?.Invoke();
    }

    public static event Action? Raised;
}