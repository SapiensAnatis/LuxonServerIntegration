using Luxon.Ser;
using LuxonServer.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class JoinGameRequest : IJoinGameRequest
{
    public byte OperationCode
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public Dictionary<byte, Value> Parameters
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public int ActorNr
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public required IDictionary<object, object?> ActorProperties { get; internal set; }
}
