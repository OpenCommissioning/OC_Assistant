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
    /// <inheritdoc cref="ConcurrentQueue{T}.ToArray"/>
    /// <inheritdoc cref="ConcurrentQueue{T}.Clear"/>
    /// </summary>
    /// <returns>
    /// <inheritdoc cref="ConcurrentQueue{T}.ToArray"/>
    /// </returns>
    public IEnumerable<T> GetAndReset()
    {
        var array = ToArray();
        Clear();
        return array;
    }
}