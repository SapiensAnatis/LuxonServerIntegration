using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

[MessagePackObject(AllowPrivate = true)]
internal sealed class HeroParamExData
{
    [Key(0)]
    public int SequenceNumber { get; set; }

    [Key(1)]
    public int LimitOverCount { get; set; }

    [Key(2)]
    public int BrSpecialSkillId { get; set; }
}
