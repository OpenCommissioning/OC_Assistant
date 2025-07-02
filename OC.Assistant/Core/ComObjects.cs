using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace OC.Assistant.Core;

/// <summary>
/// A utility class that manages a collection of COM objects used across TwinCAT operations.
/// Ensures that COM objects are tracked and can be properly released when no longer needed to prevent resource leaks.
/// </summary>
public static class ComObjects
{
    private static ConcurrentBag<object> Objects { get; } = [];

    /// <summary>
    /// Adds a COM object to the collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object being added.
    /// </typeparam>
    /// <param name="comObject">
    /// The COM object to add to the collection. If the object is null or not a COM object, it will not be added.
    /// </param>
    public static void Add<T>(T? comObject) where T : class
    {
        if (comObject is null || !Marshal.IsComObject(comObject)) return;
        Objects.Add(comObject);
    }

    /// <summary>
    /// Releases all tracked COM objects within the collection.
    /// This method ensures that all COM objects being tracked are properly
    /// released to free up unmanaged resources.
    /// </summary>
    public static void ReleaseAll()
    {
        foreach (var obj in Objects.Reverse())
        {
            Marshal.ReleaseComObject(obj);
        }
        
        Objects.Clear();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}