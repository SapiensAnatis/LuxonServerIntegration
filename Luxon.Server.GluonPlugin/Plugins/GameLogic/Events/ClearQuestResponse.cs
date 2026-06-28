using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

[MessagePackObject(AllowPrivate = true)]
internal sealed class ClearQuestResponse : EventBase<ClearQuestResponse>
{
    [Key(1)]
    public required byte[] RecordMultiResponse { get; set; }

    [Key(2)]
    public int ClearTime { get; set; }

    [Key(3)]
    public bool IsIgnoreRanking { get; set; }
}
