using MessagePack;

namespace Luxon.Server.GluonPlugin.Shared.Models;

[MessagePackObject(AllowPrivate = true)]
internal sealed class RoomEntryCondition
{
    [MessagePackObject(AllowPrivate = true)]
    public struct ObjectiveData
    {
        [Key(0)]
        public int TextId { get; set; }
    }

    [Key(0)]
    public required HashSet<int> UnacceptedElementals { get; set; }

    [Key(1)]
    public required HashSet<int> UnacceptedWeapons { get; set; }

    [Key(2)]
    public int RequiredPower { get; set; }

    [Key(3)]
    public ObjectiveData Objective { get; set; }
}
