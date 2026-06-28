using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class BeforeSetPropertiesCallInfo : CallInfo, IBeforeSetPropertiesCallInfo
{
    public required ISetPropertiesRequest Request { get; set; }

    public int ActorNr { get; set; }
}
