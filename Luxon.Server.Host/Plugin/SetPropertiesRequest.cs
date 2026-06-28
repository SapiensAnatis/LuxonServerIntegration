using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class SetPropertiesRequest : ISetPropertiesRequest
{
    public required int ActorNumber { get; init; }

    public required IDictionary<object, object?> Properties { get; init; }
}
