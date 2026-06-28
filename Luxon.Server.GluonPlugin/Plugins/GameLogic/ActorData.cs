using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

[MessagePackObject(AllowPrivate = true)]
internal struct ActorData
{
    [Key(0)]
    public int ActorId { get; set; }

    [Key(1)]
    public ulong ViewerId { get; set; }
}