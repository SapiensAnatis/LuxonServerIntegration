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
            return new Value(21, MakeLuxonDictionary(dictionary));
        }
    }

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

    private static Value MakeLuxonDictionary<TKey>(IDictionary<TKey, object?> managedDictionary)
    {
        var dict = managedDictionary.ToDictionary(
            x => Value.FromManaged(x.Key),
            x => Value.FromManaged(x.Value)
        );

        return Value.FromManaged(dict);
    }
}
