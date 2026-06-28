using Luxon.Ser;

namespace Luxon.Server.Host.Helpers;

internal static class ValueExtensions
{
    extension(Value)
    {
        public static Value FromManaged(object? managedValue)
        {
            return new Value(GetTypeTag(managedValue), managedValue);
        }

        public static Value FromManagedDictionary<TKey>(IDictionary<TKey, object?> dictionary)
        {
            var luxonDict = MakeLuxonDictionary(dictionary);
            return Value.FromManaged(luxonDict);
        }
    }

    private static int GetTypeTag(object? value)
    {
        if (value == null)
        {
            return IpcValueTypes.Null;
        }

        Type type = value.GetType();

        if (type == typeof(int))
        {
            return IpcValueTypes.Int32;
        }

        if (type == typeof(string))
        {
            return IpcValueTypes.String;
        }

        if (type == typeof(byte[]))
        {
            return IpcValueTypes.ByteArray;
        }

        if (type.IsAssignableTo(typeof(IDictionary<Value, Value>)))
        {
            return IpcValueTypes.Hashtable;
        }

        throw new NotImplementedException($"Cannot (yet) convert {type} to a Luxon value.");
    }

    private static Dictionary<Value, Value> MakeLuxonDictionary<TKey>(
        IDictionary<TKey, object?> managedDictionary
    )
    {
        return managedDictionary.ToDictionary(
            x => Value.FromManaged(x.Key),
            x => Value.FromManaged(x.Value)
        );
    }
}
