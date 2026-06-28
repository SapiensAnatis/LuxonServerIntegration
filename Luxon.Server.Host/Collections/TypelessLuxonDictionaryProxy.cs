using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;

namespace Luxon.Server.Host.Collections;

internal sealed class TypelessLuxonDictionaryProxy(IDictionary<Value, Value> luxonDictionary)
    : IDictionary<object, object?>
{
    private class Enumerator(IEnumerator<KeyValuePair<Value, Value>> luxonEnumerator)
        : IEnumerator<KeyValuePair<object, object?>>
    {
        private KeyValuePair<object, object?>? current;

        public bool MoveNext()
        {
            if (!luxonEnumerator.MoveNext())
            {
                return false;
            }

            this.current = new KeyValuePair<object, object?>(
                luxonEnumerator.Current.Key.Content,
                luxonEnumerator.Current.Value.Content
            );
            return true;
        }

        public void Reset()
        {
            luxonEnumerator.Reset();
        }

        KeyValuePair<object, object?> IEnumerator<KeyValuePair<object, object?>>.Current
        {
            get => this.current ?? throw new InvalidOperationException("Must call MoveNext first");
        }

        object? IEnumerator.Current => ((IEnumerator<KeyValuePair<object, object?>>)this).Current;

        public void Dispose()
        {
            luxonEnumerator.Dispose();
        }
    }

    public IEnumerator<KeyValuePair<object, object?>> GetEnumerator()
    {
        return new Enumerator(luxonDictionary.GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public void Add(KeyValuePair<object, object?> item)
    {
        this.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        luxonDictionary.Clear();
    }

    public bool Contains(KeyValuePair<object, object?> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<object, object?>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<object, object?> item)
    {
        throw new NotImplementedException();
    }

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(object key, object? value)
    {
        var luxonKey = new Value(GetTypeTag(key), key);
        var luxonValue = new Value(GetTypeTag(value), value);
        luxonDictionary.Add(luxonKey, luxonValue);
    }

    public bool ContainsKey(object key)
    {
        return luxonDictionary.ContainsKey(new Value(GetTypeTag(key), key));
    }

    public bool Remove(object key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(object key, out object? value)
    {
        var luxonKey = new Value(GetTypeTag(key), key);
        if (luxonDictionary.TryGetValue(luxonKey, out var luxonValue))
        {
            value = luxonValue.Content;
            return true;
        }

        value = null;
        return false;
    }

    public object? this[object key]
    {
        get
        {
            var luxonKey = new Value(GetTypeTag(key), key);
            var luxonValue = luxonDictionary[luxonKey];

            return luxonValue.Content;
        }
        set
        {
            var luxonKey = new Value(GetTypeTag(key), key);
            var luxonValue = new Value(GetTypeTag(value), value);

            luxonDictionary[luxonKey] = luxonValue;
        }
    }

    public ICollection<object> Keys => throw new NotImplementedException();

    public ICollection<object?> Values => throw new NotImplementedException();

    private static int GetTypeTag(object? value)
    {
        if (value == null)
        {
            return 0;
        }

        Type type = value.GetType();

        if (type == typeof(int))
        {
            return 4;
        }

        if (type == typeof(string))
        {
            return 8;
        }

        throw new NotImplementedException();
    }
}
