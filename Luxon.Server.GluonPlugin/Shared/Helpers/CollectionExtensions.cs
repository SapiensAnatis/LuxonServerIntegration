using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Luxon.Server.GluonPlugin.Shared.Constants;

namespace Luxon.Server.GluonPlugin.Shared.Helpers;

internal static class CollectionExtensions
{
    extension(IDictionary<object, object?> hashtable)
    {
        public int GetInt(string key)
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

        public long GetLong(string key)
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

        public bool TryGetInt(string key, out int value)
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

        public bool TryGetLong(string key, out long value)
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

        public void InitializeViewerId()
        {
            if (!hashtable.ContainsKey(ActorPropertyKeys.ViewerId))
            {
                hashtable.Add(ActorPropertyKeys.ViewerId, hashtable[ActorPropertyKeys.PlayerId]);
            }
        }
    }

    extension(IDictionary<object, object?>? hashtable)
    {
        public bool TryGetValue(string key, [NotNullWhen(true)] out object? value)
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
}
