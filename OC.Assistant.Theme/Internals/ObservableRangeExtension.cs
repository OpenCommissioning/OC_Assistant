using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace OC.Assistant.Theme.Internals;

/// <inheritdoc />
internal class ObservableRangeExtension<T> : ObservableCollection<T>
{
    /// <summary> 
    /// Adds the elements of the specified collection. 
    /// </summary> 
    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var i in collection) Items.Add(i);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary> 
    /// Removes a range of elements.
    /// </summary> 
    public void RemoveRange(int index, int count)
    {
        for (var i = index; i < index + count; i++) Items.RemoveAt(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}