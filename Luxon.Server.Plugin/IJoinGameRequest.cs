using Luxon.Ser;

namespace Luxon.Server.Plugin;

public interface IJoinGameRequest : IOperationRequest
{
    int ActorNr { get; }

    IDictionary<object, object?> ActorProperties { get; }

    [Obsolete("Not implemented yet")]
    int EmptyRoomLiveTime { get; set; }

    IDictionary<object, object?> GameProperties { get; }
}
