using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

[MessagePackObject(AllowPrivate = true)]
internal sealed class FailQuestRequest : EventBase<FailQuestRequest>
{
    public enum FailTypes
    {
        Timeup,
        AllDead,
    }

    [Key(1)]
    public FailTypes FailType { get; set; }
}
