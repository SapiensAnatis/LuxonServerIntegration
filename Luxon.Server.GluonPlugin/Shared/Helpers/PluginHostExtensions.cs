using System.Text.Json;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.Plugin;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Shared.Helpers;

internal static partial class PluginHostExtensions
{
    private const int EventDataKey = 245;
    private const int EventActorNrKey = 254;

    private static readonly MessagePackSerializerOptions MessagePackOptions =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

    extension(IPluginHost pluginHost)
    {
        /// <summary>
        /// Helper method to raise events.
        /// </summary>
        /// <param name="eventCode">The event code to raise.</param>
        /// <param name="eventData">The event data.</param>
        /// <param name="target">The actor to target -- if null, all actors will be targeted.</param>
        public void RaiseMsgpackEvent(Event eventCode, object eventData, int? target = null)
        {
            byte[] serializedEvent = MessagePackSerializer.Serialize(eventData, MessagePackOptions);
            var props = new Dictionary<byte, object?>()
            {
                { EventDataKey, serializedEvent },
                { EventActorNrKey, 0 },
            };

            LogRaisingEvent(pluginHost.Logger, eventCode, eventCode);
            LogEventData(pluginHost.Logger, eventData);

            if (target is null)
            {
                pluginHost.BroadcastEvent(
                    ReceiverGroup.All,
                    0,
                    0,
                    (byte)eventCode,
                    props,
                    new SendParameters { DeliveryMode = LuxonDeliveryMode.Reliable }
                );
            }
            else
            {
                LogEventTarget(pluginHost.Logger, target);
                pluginHost.BroadcastEvent(
                    [target.Value],
                    0,
                    (byte)eventCode,
                    props,
                    new SendParameters { DeliveryMode = LuxonDeliveryMode.Reliable }
                );
            }
        }

        public bool GetIsSoloPlay() =>
            pluginHost.GameProperties.TryGetValue(
                GamePropertyKeys.IsSoloPlayWithPhoton,
                out object? isSoloPlay
            ) && isSoloPlay is true;

        public int GetQuestId() => pluginHost.GameProperties.GetInt(GamePropertyKeys.QuestId);
    }

    [LoggerMessage(LogLevel.Information, "Raising event {EventCode} (0x{Event:X})")]
    static partial void LogRaisingEvent(ILogger logger, Event eventCode, Event @event);

    [LoggerMessage(LogLevel.Debug, "Event data: {@EventData}")]
    static partial void LogEventData(ILogger logger, object eventData);

    [LoggerMessage(LogLevel.Debug, "Event will target actor {Target}")]
    static partial void LogEventTarget(ILogger logger, int? target);
}
