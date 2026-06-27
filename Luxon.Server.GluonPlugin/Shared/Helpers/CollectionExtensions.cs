using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Luxon.Server.GluonPlugin.Shared.Constants;

namespace Luxon.Server.GluonPlugin.Shared.Helpers;

internal static class CollectionExtensions
{
    public static int GetInt(this IDictionary<object, object?> hashtable, string key)
    {
        if (!hashtable.TryGetInt(key, out int value))
        {
            throw new ArgumentException(
                $"IDictionary<object, object?> did not contain required integer value for key {key}",
                nameof(key)
            );
        }

        return value;
    }

    public static long GetLong(this IDictionary<object, object?> hashtable, string key)
    {
        if (!hashtable.TryGetLong(key, out long value))
        {
            throw new ArgumentException(
                $"IDictionary<object, object?> did not contain required integer value for key {key}",
                nameof(key)
            );
        }

        return value;
    }

    public static bool TryGetValue(
        this IDictionary<object, object?>? hashtable,
        string key,
        [NotNullWhen(true)] out object? value
    )
    {
        value = null;

        if (hashtable is null)
        {
            return false;
        }

        value = hashtable[key];

        if (value is null)
        {
            return false;
        }

        return true;
    }

    public static bool TryGetInt(
        this IDictionary<object, object?> hashtable,
        string key,
        out int value
    )
    {
        value = 0;

        if (!hashtable.TryGetValue(key, out object? objValue))
        {
            return false;
        }

        if (objValue is int intValue)
        {
            value = intValue;
            return true;
        }
        else if (objValue is string stringValue)
        {
            value = int.Parse(stringValue, CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }

    public static bool TryGetLong(
        this IDictionary<object, object?> hashtable,
        string key,
        out long value
    )
    {
        value = 0;

        if (!hashtable.TryGetValue(key, out object? objValue))
        {
            return false;
        }

        if (objValue is long longValue)
        {
            value = longValue;
            return true;
        }
        else if (objValue is string stringValue)
        {
            value = long.Parse(stringValue, CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }

    // public static bool TryGetInt(this PropertyBag<object>? properties, string key, out int value)
    // {
    //     value = 0;
    //
    //     if (properties is null || !properties.TryGetValue(key, out object objValue))
    //     {
    //         return false;
    //     }
    //
    //     if (objValue is int intValue)
    //     {
    //         value = intValue;
    //         return true;
    //     }
    //     else if (objValue is string stringValue)
    //     {
    //         value = int.Parse(stringValue);
    //         return true;
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }

    // public static int GetInt(this PropertyBag<object> properties, string key)
    // {
    //     if (!properties.TryGetInt(key, out int value))
    //     {
    //         throw new ArgumentException(
    //             $"PropertyBag did not contain required integer value for key {key}",
    //             nameof(key)
    //         );
    //     }
    //
    //     return value;
    // }
    //
    // public static int GetIntOrDefault(this PropertyBag<object> properties, string key)
    // {
    //     if (!properties.TryGetInt(key, out int value))
    //     {
    //         return 0;
    //     }
    //
    //     return value;
    // }
    //
    // public static bool TryGetBool(this PropertyBag<object>? properties, string key, out bool value)
    // {
    //     value = false;
    //
    //     if (properties is null || !properties.TryGetValue(key, out object objValue))
    //     {
    //         return false;
    //     }
    //
    //     if (objValue is bool boolValue)
    //     {
    //         value = boolValue;
    //         return true;
    //     }
    //
    //     return false;
    // }

    public static void InitializeViewerId(this IDictionary<object, object?> actorProperties)
    {
        if (!actorProperties.ContainsKey(ActorPropertyKeys.ViewerId))
        {
            actorProperties.Add(
                ActorPropertyKeys.ViewerId,
                actorProperties[ActorPropertyKeys.PlayerId]
            );
        }
    }
}
