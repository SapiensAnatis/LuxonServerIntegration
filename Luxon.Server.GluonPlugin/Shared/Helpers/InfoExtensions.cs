using Luxon.Ser;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Luxon.Server.Plugin;
using MessagePack;

namespace Luxon.Server.GluonPlugin.Shared.Helpers;

internal static class InfoExtensions
{
    private const int EventDataKey = 245;

    extension(IRaiseEventCallInfo info)
    {
        public TEvent DeserializeEvent<TEvent>()
            where TEvent : EventBase<TEvent>
        {
            if (
                !info.Request.Parameters.TryGetValue(EventDataKey, out Value value)
                || value is not { Content: byte[] blob }
            )
            {
                throw new ArgumentException(
                    "Attempted to deserialize event without event data property",
                    nameof(info)
                );
            }

            return MessagePackSerializer.Deserialize<TEvent>(
                blob,
                MessagePackSerializerOptions.Standard.WithCompression(
                    MessagePackCompression.Lz4Block
                )
            );
        }
    }
}
