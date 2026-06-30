using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DragaliaAPI.Photon.Shared.Models;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.GluonPlugin.Shared.Helpers;
using Luxon.Server.Plugin;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

internal sealed partial class GoToIngameStateManager
{
    private static readonly MessagePackSerializerOptions MessagePackOptions =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IPluginHost pluginHost;
    private readonly PluginStateService pluginStateService;
    private readonly PluginConfiguration pluginConfiguration;
    private readonly ILogger<GoToIngameStateManager> logger;
    private readonly Dictionary<int, HeroParamState> heroParamStorage;

    public int MinGoToIngameState { get; private set; }

    public GoToIngameStateManager(
        IPluginHost pluginHost,
        PluginStateService pluginStateService,
        PluginConfiguration pluginConfiguration
    )
    {
        this.pluginHost = pluginHost;
        this.pluginStateService = pluginStateService;
        this.pluginConfiguration = pluginConfiguration;

        this.heroParamStorage = new Dictionary<int, HeroParamState>(4);
        this.logger = this.pluginHost.CreateLogger<GoToIngameStateManager>();
    }

    public async Task OnSetGoToIngameState(
        IBeforeSetPropertiesCallInfo info,
        int value,
        CancellationToken cancellationToken
    )
    {
        int minValue = this
            .pluginHost.GameActors.Where(x => x.ActorNr != info.ActorNr) // Exclude the value which we are in the BeforeSet handler for
            .Select(x => x.Properties.GetIntOrDefault(ActorPropertyKeys.GoToIngameState))
            .Append(value)
            .Min();

        this.LogReceivedGoToIngameState(value, info.ActorNr);

        this.LogCalculatedMinimumValue(minValue, this.MinGoToIngameState);

        if (minValue > this.MinGoToIngameState)
        {
            this.MinGoToIngameState = minValue;
            await this.OnMinStateChange(cancellationToken);
        }
        else if (value == 1 && info.ActorNr == 1)
        {
            this.MinGoToIngameState = 1;
            await this.OnMinStateChange(cancellationToken);
        }
        else if (value == 0 && this.pluginHost.GetIsSoloPlay())
        {
            // For time attack solo quests that don't have a lobby phase
            this.SetGoToIngameInfo();
            this.pluginHost.RaiseMsgpackEvent(Event.StartQuest, new Dictionary<string, object>());
        }
    }

    public int GetUsedMemberCount(int actorNr)
    {
        return this.heroParamStorage[actorNr].UsedMemberCount;
    }

    /// <summary>
    /// Handler to perform various operations when the minimum GoToIngameState of a room is increased.
    /// </summary>
    /// <remarks>
    /// Represents various stages of loading into a quest, during which events/properties need to be raised/set.
    /// </remarks>
    private async Task OnMinStateChange(CancellationToken cancellationToken)
    {
        this.LogUpdatingMinState(this.MinGoToIngameState);

        switch (this.MinGoToIngameState)
        {
            case 1:
                this.SetGoToIngameInfo();
                break;
            case 2:
                await this.RequestHeroParam(cancellationToken);
                break;
            case 3:
                this.RaisePartyEvent();
                break;
            case 4:
                this.RaiseCharacterDataEvent();
                break;
        }
    }

    /// <summary>
    /// Sets the GoToIngameInfo room property by gathering data from connected actors.
    /// </summary>
    private void SetGoToIngameInfo()
    {
        IEnumerable<ActorData> actorData = this.pluginHost.GameActors.Select(x => new ActorData()
        {
            ActorId = x.ActorNr,
            ViewerId = (ulong)x.GetViewerId(),
        });

        var data = new GoToIngameState { Elements = actorData, BrInitData = null };

        byte[] msgpack = MessagePackSerializer.Serialize(data, MessagePackOptions);

        this.pluginHost.SetProperties(
            0, // Set properties on the game itself
            new Dictionary<object, object?> { { GamePropertyKeys.GoToIngameInfo, msgpack } },
            null,
            true
        );
    }

    /// <summary>
    /// Makes an outgoing request for <see cref="HeroParamData"/> for each player in the room.
    /// </summary>
    private async Task RequestHeroParam(CancellationToken cancellationToken)
    {
        var heroParamRequest = this
            .pluginHost.GameActors.Select(x => new ActorInfo()
            {
                QuestId = this.pluginHost.GetQuestId(),
                ActorNr = x.ActorNr,
                ViewerId = x.GetViewerId(),
                PartySlots = x.GetPartySlots(),
            })
            .ToList();

        var baseUri = this.pluginConfiguration.ApiServerUrl;
        var requestUri = new Uri(baseUri, "2.19.0_20220714193707/heroparam/batch");

        this.LogRequestingHeroParam(requestUri.AbsoluteUri);

        var response = await this.pluginHost.HttpClient.PostAsJsonAsync(
            requestUri.AbsoluteUri,
            heroParamRequest,
            JsonOptions,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            this.LogHeroParamRequestFailed(response.StatusCode);
            return;
        }

        var responseObject =
            await response.Content.ReadFromJsonAsync<List<HeroParamData>>(cancellationToken) ?? [];

        this.LogReceivedHeroParam(responseObject);

        foreach (var data in responseObject)
        {
            this.heroParamStorage[data.ActorNr] = new HeroParamState(data);
        }
    }

    /// <summary>
    /// Raises the <see cref="Event.Party"/> event.
    /// </summary>
    private void RaisePartyEvent()
    {
        Dictionary<int, int> memberCountTable = this.GetMemberCountTable();

        foreach (HeroParamState state in this.heroParamStorage.Values)
        {
            state.UsedMemberCount = memberCountTable[state.ActorNr];
        }

        int questId = this.pluginHost.GetQuestId();
        int rankingType = QuestHelper.GetIsRanked(questId) ? 1 : 0;

        var evt = new PartyEvent()
        {
            MemberCountTable = memberCountTable,
            ReBattleCount = this.pluginConfiguration.ReplayTimeoutSeconds,
            RankingType = rankingType,
        };

        this.pluginHost.RaiseMsgpackEvent(Event.Party, evt);
    }

    /// <summary>
    /// Gets how many units each actor should control based on the number of ingame players.
    /// </summary>
    /// <returns>A dictionary of { [actorNr] = unitCount }</returns>
    private Dictionary<int, int> GetMemberCountTable()
    {
        bool isRaid =
            this.pluginHost.GameProperties.TryGetInt(GamePropertyKeys.QuestId, out int questId)
            && QuestHelper.GetIsRaid(questId);

        if (isRaid || this.pluginHost.GetIsSoloPlay())
        {
            // Use all available units
            return this.heroParamStorage.ToDictionary(
                x => x.Value.ActorNr,
                x => x.Value.HeroParamCount
            );
        }

        return MemberCountHelper.BuildMemberCountTable(
            this.heroParamStorage.Select(x => new ValueTuple<int, int>(
                    x.Value.ActorNr,
                    x.Value.HeroParamCount
                ))
                .ToList()
        );
    }

    /// <summary>
    /// Raise <see cref="Event.CharacterData"/> using cached <see cref="HeroParamData"/>.
    /// </summary>
    private void RaiseCharacterDataEvent()
    {
        foreach (KeyValuePair<int, HeroParamState> kvp in this.heroParamStorage)
        {
            int actorNr = kvp.Key;
            HeroParamState state = kvp.Value;

            foreach (List<HeroParam> heroParams in state.Data.HeroParamLists)
            {
                var evt = new CharacterData()
                {
                    PlayerId = actorNr,
                    HeroParamExs = heroParams
                        .Select(x => new HeroParamExData()
                        {
                            LimitOverCount = x.ExAbilityLv,
                            SequenceNumber = x.Position,
                        })
                        .ToArray(),
                    HeroParams = heroParams.Take(state.UsedMemberCount).ToArray(),
                };

                this.pluginHost.RaiseMsgpackEvent(Event.CharacterData, evt);
            }
        }
    }

    [LoggerMessage(LogLevel.Information, "Received GoToIngameState {Value} from actor {ActorNr}")]
    partial void LogReceivedGoToIngameState(int value, int actorNr);

    [LoggerMessage(
        LogLevel.Debug,
        "Calculated minimum value: {MinValue}, instance minimum value {InstanceMinValue}"
    )]
    partial void LogCalculatedMinimumValue(int minValue, int instanceMinValue);

    [LoggerMessage(
        LogLevel.Information,
        "OnSetGoToIngameState: updating with value {MinGoToIngameState}"
    )]
    partial void LogUpdatingMinState(int minGoToIngameState);

    [LoggerMessage(LogLevel.Debug, "Requesting HeroParam from URL {URL}")]
    partial void LogRequestingHeroParam(string url);

    [LoggerMessage(LogLevel.Error, "HeroParam request failed with status {Status}")]
    partial void LogHeroParamRequestFailed(HttpStatusCode status);

    [LoggerMessage(LogLevel.Debug, "Received HeroParam: {@HeroParam}")]
    partial void LogReceivedHeroParam(List<HeroParamData> heroParam);
}
