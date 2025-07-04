using System.Runtime.InteropServices;

namespace OC.Assistant.Core;

/// <summary>
/// Represents a static class to manage COM objects.
/// COM objects can be tracked and released when no longer needed to prevent resource leaks.
/// </summary>
public static class ComHelper
{
    private static HashSet<object> TrackedObjects { get; } = [];
    private static readonly object Lock = new();
    
    /// <summary>
    /// Releases the references of the given COM object.
    /// </summary>
    /// <typeparam name="T">The type of the object being released.</typeparam>
    /// <param name="comObject">The COM object to release.</param>
    public static void ReleaseObject<T>(T? comObject) where T : class
    {
        if (comObject is null || !Marshal.IsComObject(comObject)) return;
        Marshal.FinalReleaseComObject(comObject);
    }

    /// <summary>
    /// Adds a COM object to the collection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object being added.
    /// </typeparam>
    /// <param name="comObject">
    /// The COM object to add to the collection. If the object is null or not a COM object, it will not be added.
    /// </param>
    public static void TrackObject<T>(T? comObject) where T : class
    {
        if (comObject is null || !Marshal.IsComObject(comObject)) return;
        lock (Lock)
        {
            TrackedObjects.Add(comObject);
        }
    }

    /// <summary>
    /// Releases the references of all tracked COM objects and forces an immediate garbage collection,
    /// ensuring that all tracked COM objects are properly released.
    /// </summary>
    public static void ReleaseTrackedObjects()
    {
        lock (Lock)
        {
            foreach (var obj in TrackedObjects)
            {
                Marshal.FinalReleaseComObject(obj);
            }
        
            TrackedObjects.Clear();
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}