using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

[MessagePackObject(AllowPrivate = true)]
internal struct GoToIngameState
{
    [Key(0)]
    public IEnumerable<ActorData> Elements { get; set; }

    [Key(1)]
    public BrInitData? BrInitData { get; set; }
}
