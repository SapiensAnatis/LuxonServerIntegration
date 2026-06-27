namespace Luxon.Server.Plugin;

public interface IJoinGameCallInfo : ITypedCallInfo<IJoinGameRequest>
{
    int ActorNr { get; }
}
