using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class JoinGameCallInfo : CallInfo, IJoinGameCallInfo
{
    public required IJoinGameRequest Request { get; init; }

    public required int ActorNr { get; init; }
}
