using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class CreateGameCallInfo : CallInfo, ICreateGameCallInfo
{
    public required IJoinGameRequest Request { get; init; }

    public bool CreateIfNotExists { get; init; }
}
