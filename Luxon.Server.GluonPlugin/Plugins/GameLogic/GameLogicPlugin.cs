using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DragaliaAPI.Photon.Shared.Enums;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.GluonPlugin.Shared.Helpers;
using Luxon.Server.Plugin;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.GameLogic;

internal sealed partial class GameLogicPlugin : PluginBase
{
    private static readonly MessagePackSerializerOptions EventMessagePackOptions =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

    private RoomState roomState;
    private readonly PluginConfiguration configuration;
    private readonly PluginStateService pluginStateService;
    private readonly Dictionary<int, ActorState> actorState;

    private GoToIngameStateManager goToIngameStateManager;

    private readonly ILogger<GameLogicPlugin> logger;

    public GameLogicPlugin(
        IPluginHost host,
        PluginConfiguration configuration,
        PluginStateService pluginStateService
    )
        : base(host)
    {
        this.roomState = new RoomState();
        this.actorState = new Dictionary<int, ActorState>();
        this.configuration = configuration;
        this.pluginStateService = pluginStateService;

        this.goToIngameStateManager = new GoToIngameStateManager(
            host,
            this.pluginStateService,
            this.configuration
        );

        this.logger = host.CreateLogger<GameLogicPlugin>();
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
            case Event.ClearQuestRequest:
                await this.OnClearQuestRequest(info, cancellationToken);
                break;
            case Event.GameSucceed:
                this.OnGameSucceed(info);
                break;
            case Event.FailQuestRequest:
                this.OnFailQuestRequest(info);
                break;
            case Event.Dead:
                // !!! TODO: How does this behave with AI units?
                this.actorState[info.ActorNr].Dead = true;
                break;
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
            await this.goToIngameStateManager.OnSetGoToIngameState(info, value, cancellationToken);
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
            this.PluginHost.RaiseMsgpackEvent(Event.StartQuest, new Dictionary<string, string>());

            this.roomState.StartActorCount = this.PluginHost.GameActors.Count;
        }
    }

    /// <summary>
    /// Handler for when the client raises <see cref="Event.ClearQuestRequest"/>.
    /// </summary>
    /// <param name="info">Event call info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task OnClearQuestRequest(
        IRaiseEventCallInfo info,
        CancellationToken cancellationToken
    )
    {
        // These properties must be set for the client to successfully rejoin the room.
        int roomId = this.PluginHost.GameProperties.GetInt(GamePropertyKeys.RoomId);

        if (roomId > 0)
        {
            this.PluginHost.SetProperties(
                0,
                new Dictionary<object, object?>()
                {
                    { GamePropertyKeys.GoToIngameInfo, null },
                    { GamePropertyKeys.RoomId, -GenerateRoomId() },
                },
                null,
                true
            );
        }

        this.actorState[info.ActorNr] = new ActorState();

        if (info.Request.Data is not byte[] byteArray)
        {
            throw new InvalidOperationException(
                "Failed to get byte array from ClearQuestRequest event"
            );
        }

        var request = MessagePackSerializer.Deserialize<ClearQuestRequest>(
            byteArray,
            EventMessagePackOptions,
            cancellationToken
        );

        var augmentedRequest = this.AugmentQuestClearRequest(
            request.RecordMultiRequest,
            info.ActorNr
        );

        IActor actor = this.PluginHost.GameActors.First(x => x.ActorNr == info.ActorNr);

        using var content = new ByteArrayContent(augmentedRequest);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.Add(
            "Auth-ViewerId",
            actor.GetViewerId().ToString(CultureInfo.InvariantCulture)
        );

        var dungeonResponse = await this.PluginHost.HttpClient.PostAsync(
            this.configuration.DungeonRecordMultiEndpoint,
            content,
            cancellationToken
        );

        dungeonResponse.EnsureSuccessStatusCode();

        if (this.ShouldRegisterTimeAttack())
        {
            this.LogRegisteringTimeAttackClear();

            var timeAttackResponse = await this.PluginHost.HttpClient.PostAsync(
                this.configuration.TimeAttackEndpoint,
                content,
                cancellationToken
            );

            if (!timeAttackResponse.IsSuccessStatusCode)
            {
                this.LogTimeAttackRecordRequestFailed(timeAttackResponse.StatusCode);
            }
        }

        this.PluginHost.RaiseMsgpackEvent(
            Event.ClearQuestResponse,
            new ClearQuestResponse()
            {
                RecordMultiResponse = await dungeonResponse.Content.ReadAsByteArrayAsync(
                    cancellationToken
                ),
            },
            info.ActorNr
        );
    }

    /// <summary>
    /// Handler for when a client calls <see cref="Event.GameSucceed"/>.
    /// </summary>
    /// <param name="info">Info from <see cref="OnRaiseEvent(IRaiseEventCallInfo,CancellationToken)"/>.</param>
    private void OnGameSucceed(IRaiseEventCallInfo info)
    {
        this.LogReceivedGameSucceed(info.ActorNr);

        if (info.ActorNr == 1)
        {
            this.roomState = new RoomState(this.roomState);
            this.goToIngameStateManager = new GoToIngameStateManager(
                this.PluginHost,
                this.pluginStateService,
                this.configuration
            );

            this.PluginHost.RaiseMsgpackEvent(Event.GameSucceed, new { });
            this.PluginHost.SetProperties(
                0,
                new Dictionary<object, object?>() { [GamePropertyKeys.RoomId] = GenerateRoomId() },
                null,
                true
            );
        }
    }

    /// <summary>
    /// Handler for when the client calls <see cref="Event.FailQuestRequest"/>.
    /// </summary>
    /// <param name="info">Event call info.</param>
    private void OnFailQuestRequest(IRaiseEventCallInfo info)
    {
        this.actorState[info.ActorNr].Ready = false;

        this.PluginHost.SetProperties(
            info.ActorNr,
            new Dictionary<object, object?>() { { ActorPropertyKeys.GoToIngameState, 0 } },
            null,
            false
        );

        if (info.Request.Data is not byte[] byteArray)
        {
            throw new InvalidOperationException(
                "Failed to get byte array from FailQuestRequest event"
            );
        }

        FailQuestRequest request = MessagePackSerializer.Deserialize<FailQuestRequest>(
            byteArray,
            EventMessagePackOptions
        );

        this.LogReceivedFailQuestRequest(request.FailType);

        var response = new FailQuestResponse()
        {
            ResultType =
                request.FailType == FailQuestRequest.FailTypes.Timeup
                    ? FailQuestResponse.ResultTypes.Timeup
                    : FailQuestResponse.ResultTypes.Clear,
        };

        this.PluginHost.RaiseMsgpackEvent(Event.FailQuestResponse, response, info.ActorNr);

        if (
            this.PluginHost.GameActors.Count < this.roomState.StartActorCount
            || this.actorState.All(x => x.Value.Dead)
        )
        {
            // Return to lobby
            this.LogFailQuestReturningToLobby();
            this.actorState[info.ActorNr] = new ActorState();

            this.PluginHost.SetProperties(
                0,
                new Dictionary<object, object?>()
                {
                    { GamePropertyKeys.GoToIngameInfo, null },
                    { GamePropertyKeys.RoomId, -1 },
                },
                null,
                true
            );
        }

        this.roomState = new RoomState(this.roomState);
        this.goToIngameStateManager = new GoToIngameStateManager(
            this.PluginHost,
            this.pluginStateService,
            this.configuration
        );
    }

    private byte[] AugmentQuestClearRequest(byte[] original, int actorNr)
    {
        Dictionary<string, object?> deserialized = MessagePackSerializer.Deserialize<
            Dictionary<string, object?>
        >(original);

        deserialized["connecting_viewer_id_list"] = this
            .PluginHost.GameActors.Where(x => x.ActorNr != actorNr)
            .OrderBy(x => x.ActorNr)
            .Select(x => x.GetViewerId())
            .ToArray();

        deserialized["is_host"] = actorNr == 1;

        deserialized["member_count"] = this.goToIngameStateManager.GetUsedMemberCount(actorNr);

        IActor actor = this.PluginHost.GameActors.First(x => x.ActorNr == actorNr);

        if (actor.Properties.TryGetInt(ActorPropertyKeys.AstralBetCount, out int astralBetCount))
        {
            deserialized["astral_bet_count"] = astralBetCount;
        }

        return MessagePackSerializer.Serialize(deserialized);
    }

    private bool ShouldRegisterTimeAttack()
    {
        if (!QuestHelper.GetIsRanked(this.roomState.QuestId))
        {
            this.LogNotRegisteringTaClear(this.roomState.QuestId);
            return false;
        }

        if (!this.roomState.IsSoloPlay && this.PluginHost.GameActors.Count < 4)
        {
            this.LogNotRegisteringNonFullTaClear(this.PluginHost.GameActors.Count);
            return false;
        }

        return true;
    }

#pragma warning disable CA5394 // Do not use insecure randomness. This value is not security-sensitive.
    private static int GenerateRoomId() => Random.Shared.Next(100_0000, 1_000_0000);
#pragma warning restore CA5394
}
