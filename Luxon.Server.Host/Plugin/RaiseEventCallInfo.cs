using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class RaiseEventCallInfo : CallInfo, IRaiseEventCallInfo
{
    public required IRaiseEventRequest Request { get; init; }

    public required int ActorNr { get; init; }
}
