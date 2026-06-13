using System.Collections;
using Luxon.Ser;

namespace LuxonServer.Plugin;

public interface IJoinGameRequest : IOperationRequest
{
    int ActorNr { get; }
    IDictionary<object, object?> ActorProperties { get; }
}
