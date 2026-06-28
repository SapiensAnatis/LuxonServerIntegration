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

        if (type.IsAssignableTo(typeof(IDictionary<Value, Value>)))
        {
            return 21;
        }

        throw new NotImplementedException();
    }
}
