using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;

namespace Luxon.Server.Host.Collections;

internal sealed class TypelessLuxonDictionaryProxy : IDictionary<object, object?>
{
    private readonly IDictionary<Value, Value> _luxonDictionary;

    public TypelessLuxonDictionaryProxy(IDictionary<Value, Value> luxonDictionary)
    {
        _luxonDictionary = luxonDictionary;
    }

    public IEnumerator<KeyValuePair<object, object?>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<object, object?> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
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
        _luxonDictionary.Add(luxonKey, luxonValue);
    }

    public bool ContainsKey(object key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(object key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(object key, [MaybeNullWhen(false)] out object? value)
    {
        throw new NotImplementedException();
    }

    public object? this[object key]
    {
        get
        {
            var luxonKey = new Value(GetTypeTag(key), key);
            var luxonValue = _luxonDictionary[luxonKey];

            return luxonValue.Content;
        }
        set
        {
            Value luxonKey = new Value(GetTypeTag(key), key);
            Value luxonValue = new Value(GetTypeTag(value), value);

            _luxonDictionary[luxonKey] = luxonValue;
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
