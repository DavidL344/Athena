using System.Collections;

namespace Athena.Core.Model.DataStructures;

public class UniqueList<T> : IList<T>
{
    private readonly List<T> _list;
    private readonly HashSet<T> _set;

    public UniqueList()
    {
        _list = [];
        _set = [];
    }
    
    public UniqueList(IEnumerable<T> collection)
    {
        var enumerable = collection.ToArray();
        
        _list = new List<T>(enumerable);
        _set = new HashSet<T>(enumerable);
    }

    public void Add(T item)
    {
        if (_set.Contains(item)) return;
        
        _list.Add(item);
        _set.Add(item);
    }

    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public void Clear()
    {
        _list.Clear();
        _set.Clear();
    }

    public bool Contains(T item)
    {
        return _set.Contains(item);
    }

    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        if (_set.Contains(item)) return;
        
        _list.Insert(index, item);
        _set.Add(item);
    }

    public bool Remove(T item)
    {
        if (!Contains(item)) return false;
        
        return _list.Remove(item) && _set.Remove(item);
    }

    public void RemoveAt(int index)
    {
        var item = _list[index];
        _list.RemoveAt(index);
        _set.Remove(item);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            if (_set.Contains(value)) return;
            
            var item = _list[index];
            _list[index] = value;
            
            _set.Remove(item);
            _set.Add(value);
        }
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
