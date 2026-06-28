using System.Diagnostics;
using Luxon.Ser;

namespace Luxon.Server.Host.Helpers;

internal static class ValueExtensions
{
    extension(Value)
    {
        public static Value FromManaged(object? managedValue)
        {
            if (managedValue is Value)
            {
                throw new ArgumentException(
                    "Attempted to put a Luxon.Ser.Value inside a Luxon.Ser.Value",
                    nameof(managedValue)
                );
            }

            return new Value(GetTypeTag(managedValue), managedValue);
        }

        public static Value FromManagedHashtable(IDictionary<object, object?> dictionary)
        {
            var luxonDict = MakeLuxonHashtable(dictionary);
            return new Value(IpcValueTypes.Hashtable, luxonDict);
        }

        public static Value FromManagedDictionary(IDictionary<byte, object?> dictionary)
        {
            var luxonDict = MakeLuxonDictionary(dictionary);
            return new Value(IpcValueTypes.Dictionary, luxonDict);
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

        if (type == typeof(byte))
        {
            return IpcValueTypes.UInt8;
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

    private static Dictionary<Value, Value> MakeLuxonHashtable(
        IDictionary<object, object?> managedDictionary
    )
    {
        return managedDictionary.ToDictionary(
            x => Value.FromManaged(x.Key),
            x => Value.FromManaged(x.Value)
        );
    }

    private static Dictionary<byte, Value> MakeLuxonDictionary(
        IDictionary<byte, object?> managedDictionary
    )
    {
        return managedDictionary.ToDictionary(x => x.Key, x => Value.FromManaged(x.Value));
    }
}
