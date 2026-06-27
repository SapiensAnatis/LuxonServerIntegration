using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;

namespace LuxonServer.Plugin;

public interface IJoinGameRequest : IOperationRequest
{
    int ActorNr { get; }

    IDictionary<object, object?> ActorProperties { get; }

    [Obsolete("Not implemented yet")]
    int EmptyRoomLiveTime { get; set; }

    IDictionary<object, object?> GameProperties { get; }
}
