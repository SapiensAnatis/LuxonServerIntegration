using System.Collections;
using Luxon.Ser;

namespace LuxonServer.Plugin;

public interface IJoinGameRequest : IOperationRequest
{
    int ActorNr { get; set; }
    IDictionary<object, object?> ActorProperties { get; }
}
