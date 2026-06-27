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

    [Experimental("LUX0001", Message = "Not yet implemented")]
    IDictionary<object, object?> GameProperties
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}
