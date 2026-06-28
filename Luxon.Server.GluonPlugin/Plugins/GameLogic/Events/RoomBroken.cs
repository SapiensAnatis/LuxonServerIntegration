using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

[MessagePackObject(AllowPrivate = true)]
internal sealed class RoomBroken : EventBase<RoomBroken>
{
    public enum RoomBrokenType
    {
        VerifySessionError,
        HostDisconnected,
        TimeExpired,
        FailedToRegisterRanking,
    }

    [Key(1)]
    public RoomBrokenType Reason { get; set; }
}
