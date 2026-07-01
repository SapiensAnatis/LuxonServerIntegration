using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class LeaveGameCallInfo : CallInfo, ILeaveGameCallInfo
{
    public required ILeaveGameRequest Request { get; init; }

    public required int ActorNr { get; init; }

    [Obsolete("Not implemented")]
    public required int Reason { get; init; }
}
