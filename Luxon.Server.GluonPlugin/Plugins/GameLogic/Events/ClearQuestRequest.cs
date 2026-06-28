using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

[MessagePackObject(AllowPrivate = true)]
internal sealed class ClearQuestRequest : EventBase<ClearQuestRequest>
{
    [Key(1)]
    public required byte[] RecordMultiRequest { get; set; }
}
