using Luxon.Server.Plugin;
using LuxonServer.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class CreateGameCallInfo : CallInfo, ICreateGameCallInfo
{
    public required IJoinGameRequest Request { get; init; }

    public required bool CreateIfNotExists { get; init; }
}
