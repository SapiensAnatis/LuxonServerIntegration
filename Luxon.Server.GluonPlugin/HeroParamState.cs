using DragaliaAPI.Photon.Shared.Models;

namespace Luxon.Server.GluonPlugin;

public class HeroParamState
{
    public HeroParamState(HeroParamData data)
    {
        this.Data = data;
    }

    public HeroParamData Data { get; }

    public int ActorNr => Data.ActorNr;

    public int HeroParamCount => Data.HeroParamLists.FirstOrDefault()?.Count ?? 0;

    public int UsedMemberCount { get; set; }
}
