using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

[MessagePackObject(AllowPrivate = true)]
internal struct BrInitData
{
    [Key(0)]
    public int LocationPattern { get; set; }

    [Key(1)]
    public List<int> PlayerPositions { get; set; }

    [Key(2)]
    public string MultiPlayKey { get; set; }
}