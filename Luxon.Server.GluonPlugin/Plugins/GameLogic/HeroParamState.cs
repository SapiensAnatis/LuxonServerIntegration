using DragaliaAPI.Photon.Shared.Models;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

public class HeroParamState
{
    public HeroParamState(HeroParamData data)
    {
        this.Data = data;
    }

    public HeroParamData Data { get; }

    public int ActorNr => this.Data.ActorNr;

    public int HeroParamCount => this.Data.HeroParamLists.FirstOrDefault()?.Count ?? 0;

    public int UsedMemberCount { get; set; }
}
