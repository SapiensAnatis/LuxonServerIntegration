using MessagePack;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;

public abstract class EventBase<T>
{
    [Key(0)]
    public ushort RaiseEventSequenceId { get; set; } = 1;
}
