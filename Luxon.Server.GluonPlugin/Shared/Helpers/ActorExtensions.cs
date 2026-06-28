using System.Globalization;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.Plugin;

namespace Luxon.Server.GluonPlugin.Shared.Helpers;

internal static class ActorExtensions
{
    extension(IActor actor)
    {
        public bool TryGetViewerId(out long viewerId)
        {
            viewerId = 0;

            if (!actor.Properties.TryGetValue(ActorPropertyKeys.PlayerId, out object? viewerIdObj))
                return false;

            if (viewerIdObj is not string playerIdString)
                return false;

            viewerId = long.Parse(playerIdString, CultureInfo.InvariantCulture);
            return true;
        }

        public long GetViewerId()
        {
            if (!actor.TryGetViewerId(out long viewerId))
            {
                throw new InvalidOperationException(
                    $"Failed to get viewer ID for actor {actor.ActorNr}"
                );
            }

            return viewerId;
        }

        public int[] GetPartySlots()
        {
            if (
                actor.Properties.TryGetValue(ActorPropertyKeys.UsePartySlot, out object? value)
                && value is int[] partySlots
            )
            {
                return partySlots;
            }

            throw new InvalidOperationException("Failed to get party slots");
        }
    }
}
