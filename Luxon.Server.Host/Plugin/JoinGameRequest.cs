using Luxon.Ser;
using LuxonServer.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class JoinGameRequest : IJoinGameRequest
{
    public required byte OperationCode { get; set; }

    public required Dictionary<byte, Value> Parameters { get; set; }

    public required int ActorNr { get; init; }

    public required IDictionary<object, object?> ActorProperties { get; init; }
}
