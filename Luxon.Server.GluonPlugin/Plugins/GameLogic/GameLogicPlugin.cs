using DragaliaAPI.Photon.Shared.Enums;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.GluonPlugin.Shared.Helpers;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

internal sealed partial class GameLogicPlugin : PluginBase
{
    private RoomState roomState;
    private ILogger<GameLogicPlugin> logger;
    private GoToIngameStateManager goToIngameStateManager;

    private readonly PluginConfiguration configuration;
    private readonly PluginStateService pluginStateService;
    private readonly Dictionary<int, ActorState> actorState;

    public GameLogicPlugin(
        IPluginHost host,
        PluginConfiguration configuration,
        PluginStateService pluginStateService
    )
        : base(host)
    {
        this.configuration = configuration;
        this.pluginStateService = pluginStateService;

        this.actorState = new Dictionary<int, ActorState>();

        this.logger = host.CreateLogger<GameLogicPlugin>();

        this.roomState = new RoomState();

        this.goToIngameStateManager = new GoToIngameStateManager(
            host,
            this.pluginStateService,
            this.configuration
        );
    }

    public override async Task OnCreateGame(
        ICreateGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
        // Not 0 to allow for any outgoing GameLeave Redis requests to complete
        info.Request.EmptyRoomLiveTime = 500;

        this.LogGameProperties(info.Request.GameProperties);

        info.Request.ActorProperties.InitializeViewerId();

        int roomId = GenerateRoomId();
        info.Request.GameProperties.Add(GamePropertyKeys.RoomId, roomId);

        info.Continue();

        // https://doc.photonengine.com/server/current/plugins/plugins-faq#how_to_get_the_actor_number_in_plugin_callbacks_
        // This is only invalid if the room is recreated from an inactive state, which Dragalia doesn't do
        const int actorNr = 1;

        this.actorState[actorNr] = new ActorState();

        long viewerId = info.Request.ActorProperties.GetLong(ActorPropertyKeys.PlayerId);

        if (
            info.Request.GameProperties.TryGetValue(
                GamePropertyKeys.IsSoloPlayWithPhoton,
                out object? isSoloPlay
            ) && isSoloPlay is true
        )
        {
            this.LogRoomIsInSoloPlayMode();
            this.roomState.IsSoloPlay = true;
        }

        this.roomState.QuestId = info.Request.GameProperties.GetInt(GamePropertyKeys.QuestId);
        this.roomState.IsRandomMatching = QuestHelper.GetIsRandomMatching(this.roomState.QuestId);
        MatchingTypes matchingType = (MatchingTypes)
            info.Request.GameProperties.GetInt(GamePropertyKeys.MatchingType);

        this.LogGameCreated(viewerId, this.PluginHost.GameId, roomId);

        this.pluginStateService.IsPubliclyVisible =
            matchingType == MatchingTypes.Anyone && !this.roomState.IsRandomMatching;
    }

    public override async Task OnJoin(IJoinGameCallInfo info, CancellationToken cancellationToken)
    {
        int currentActorCount = this.PluginHost.GameActors.Count(x => x.ActorNr != info.ActorNr);

        if (currentActorCount >= 4)
        {
            this.LogRejectingJoinRequestGameFull(currentActorCount);

            info.Fail();
            return;
        }

        if (this.goToIngameStateManager.MinGoToIngameState > 0)
        {
            this.LogRejectingJoinGameAlreadyInProgress();

            info.Fail();
            return;
        }

        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!this.PluginHost.GameActors.Any(x => x.ActorNr == 1))
        {
            this.LogRejectingJoinGameHasNoHost();

            info.Fail();
            return;
        }

        info.Request.ActorProperties.InitializeViewerId();
        info.Continue();

        this.actorState[info.ActorNr] = new ActorState();

        long viewerId = info.Request.ActorProperties.GetLong(ActorPropertyKeys.PlayerId);

        this.LogPlayerJoined(viewerId, this.PluginHost.GameId);

        if (this.roomState.IsRandomMatching && this.roomState.RandomMatchingStartTimer == null)
        {
            // this.LogCommencingRandomMatchingStartTimerForGameGameid(this.PluginHost.GameId);
            //
            // this.roomState.RandomMatchingStartTimer = this.PluginHost.CreateOneTimeTimer(
            //     info,
            //     () =>
            //     {
            //         this.LogExecutingRandomMatchingStartForGameGameid(this.PluginHost.GameId);
            //         this.goToIngameStateManager.SetGoToIngameInfo();
            //     },
            //     this.configuration.RandomMatchingStartDelayMs
            // );

            throw new NotImplementedException("Random matching is not implemented yet");
        }
    }

    public override async Task OnRaiseEvent(
        IRaiseEventCallInfo info,
        CancellationToken cancellationToken
    )
    {
        info.Continue();

        this.LogEventRaised(info.ActorNr, (Event)info.Request.EvCode, info.Request.EvCode);

        this.LogEventProperties(info.Request.Parameters);

        switch ((Event)info.Request.EvCode)
        {
            case Event.Ready:
                this.OnActorReady(info);
                break;
            // case Event.ClearQuestRequest:
            //     this.OnClearQuestRequest(info);
            //     break;
            // case Event.GameSucceed:
            //     this.OnGameSucceed(info);
            //     break;
            // case Event.FailQuestRequest:
            //     this.OnFailQuestRequest(info);
            //     break;
            // case Event.Dead:
            //     // !!! TODO: How does this behave with AI units?
            //     this.actorState[info.ActorNr].Dead = true;
            //     break;
        }
    }

    public override async Task BeforeSetProperties(
        IBeforeSetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
        if (
            info.Request.Properties.TryGetValue(
                ActorPropertyKeys.GoToIngameState,
                out object? objValue
            ) && objValue is int value
        )
        {
            this.goToIngameStateManager.OnSetGoToIngameState(info, value);
        }

        if (!info.IsProcessed)
        {
            info.Continue();
        }
    }

    private void OnActorReady(IRaiseEventCallInfo info)
    {
        this.LogReceivedReadyEventFromActor(info.ActorNr);
        this.actorState[info.ActorNr].Ready = true;

        if (this.actorState.All(x => x.Value.Ready))
        {
            this.PluginHost.RaiseEvent(Event.StartQuest, new Dictionary<string, string>());

            this.roomState.StartActorCount = this.PluginHost.GameActors.Count;
        }
    }

#pragma warning disable CA5394 // Do not use insecure randomness. This value is not security-sensitive.
    private static int GenerateRoomId() => Random.Shared.Next(100_0000, 1_000_0000);
#pragma warning restore CA5394
}
