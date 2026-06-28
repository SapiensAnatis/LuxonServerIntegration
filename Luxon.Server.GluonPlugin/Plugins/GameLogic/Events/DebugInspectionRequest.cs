using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

[MessagePackObject(AllowPrivate = true)]
internal sealed class DebugInspectionRequest : EventBase<DebugInspectionRequest>
{
    public enum RequestTypes
    {
        IngameState,
        LeaveReason,
        AutoFailTimeout,
        PlayQuestStartProcTime,
        ChangedCharaOwner,
        AiCheckReport,
    }

    [Key(1)]
    public RequestTypes RequestType;
}
