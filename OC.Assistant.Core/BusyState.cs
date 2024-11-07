namespace OC.Assistant.Core;

/// <summary>
/// Represents a static class to set/reset a busy state.
/// Each <see cref="object"/> is responsible to reset the state after setting it, otherwise the state will remain busy.
/// </summary>
public static class BusyState
{
    private static readonly HashSet<int> HashCodes = [];
    private static readonly object HashCodesLock = new();

    /// <summary>
    /// Returns true if any <see cref="object"/> has set the state, otherwise false.
    /// </summary>
    public static bool IsSet
    {
        get
        {
            lock (HashCodesLock)
            {
                return HashCodes.Count > 0;
            }
        }
    }

    /// <summary>
    /// Checks whether the given <see cref="object"/> has set the state.
    /// </summary>
    /// <param name="sender">The <see cref="object"/> to check.</param>
    /// <returns>True if the given <see cref="object"/> has set the state, otherwise false.</returns>
    public static bool Query(object sender)
    {
        lock (HashCodesLock)
        {
            return HashCodes.Contains(sender.GetHashCode());
        }
    }
    
    /// <summary>
    /// Sets the busy state.
    /// </summary>
    /// <param name="sender">The sender from where the state is set.</param>
    public static void Set(object sender)
    {
        var hashCode = sender.GetHashCode();
        lock (HashCodesLock)
        {
            if (!HashCodes.Add(hashCode)) return;
        }
        Sdk.Logger.LogInfo(sender, $"Set busy state. Object hashcode 0x{hashCode:x8}", true);
        Changed?.Invoke(true);
    }
    
    /// <summary>
    /// Resets the busy state.
    /// <param name="sender">The sender from where the state is reset.</param>
    /// </summary>
    public static void Reset(object sender)
    {
        var hashCode = sender.GetHashCode(); 
        lock (HashCodesLock)
        {
            if (!HashCodes.Remove(hashCode)) return;
        }
        Sdk.Logger.LogInfo(sender, $"Reset busy state. Object hashcode 0x{hashCode:x8}", true);
        if (IsSet) return;
        Changed?.Invoke(false);
    }
    
    /// <summary>
    /// The state has been changed.
    /// </summary>
    public static event Action<bool>? Changed;
}