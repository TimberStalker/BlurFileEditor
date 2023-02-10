using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    List<TKey> keys = new List<TKey>();
    List<TValue> values = new List<TValue>();

    public TValue this[TKey key] 
    { 
        get => values[keys.IndexOf(key)];
        set
        {
            if(keys.Contains(key))
            {
                values[keys.IndexOf(key)] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    public ICollection<TKey> Keys => keys;

    public ICollection<TValue> Values => values;

    public int Count => keys.Count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if(keys.Contains(key)) throw new ArgumentException();

        keys.Add(key);
        values.Add(value);
    }

    public int IndexOf(TKey key) => keys.IndexOf(key);
    
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (keys.Contains(item.Key)) throw new ArgumentException();

        keys.Add(item.Key);
        values.Add(item.Value);
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return keys.Contains(item.Key) && (this[item.Key]?.Equals(item.Value) ?? false);
    }

    public bool ContainsKey(TKey key)
    {
        return keys.Contains(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for(int i = 0; i < keys.Count; i++)
        {
            yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
        }
    }

    public bool Remove(TKey key)
    {
        var index = keys.IndexOf(key);

        if(index < 0) return false;

        keys.RemoveAt(index);
        values.RemoveAt(index);

        return true;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var index = keys.IndexOf(item.Key);

        if (index < 0) return false;

        keys.RemoveAt(index);
        values.RemoveAt(index);

        return true;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if(ContainsKey(key))
        {
            value = this[key];
            return true;
        }
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
