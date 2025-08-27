using System.Collections.Concurrent;

namespace OC.Assistant.Core;

/// <inheritdoc cref="ConcurrentQueue{T}"/>
/// <param name="capacity">
/// Caps the queue size to this value.
/// </param>
public class ConcurrentQueueCapped<T>(int capacity) : ConcurrentQueue<T>
{
    /// <inheritdoc cref="ConcurrentQueue{T}.Enqueue"/><br/>
    /// Dequeues old items if the <see cref="capacity"/> is reached.
    public new void Enqueue(T item)
    {
        base.Enqueue(item);
        
        while (Count > capacity)
        {
            TryDequeue(out _);
        }
    }

    /// <summary>
    /// Dequeues and returns all objects of the concurrent queue.
    /// </summary>
    /// <returns>
    /// A list of all dequeued objects.
    /// </returns>
    public IEnumerable<T> DequeueAll()
    {
        var list = new List<T>();
        
        while (TryDequeue(out var item))
        {
            list.Add(item);
        }
        
        return list;
    }
}