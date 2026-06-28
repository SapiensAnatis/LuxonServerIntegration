using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

[MessagePackObject(AllowPrivate = true)]
internal sealed class FailQuestResponse : EventBase<FailQuestResponse>
{
    public enum ResultTypes
    {
        Timeup,
        Clear,
    }

    [Key(1)]
    public ResultTypes ResultType { get; set; }
}
