using Luxon.Server.Plugin;
using LuxonServer.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class CreateGameCallInfo(IJoinGameRequest request, bool createIfNotExists)
    : CallInfo,
        ICreateGameCallInfo
{
    public IJoinGameRequest Request { get; } = request;

    public bool CreateIfNotExists { get; set; } = createIfNotExists;
}
