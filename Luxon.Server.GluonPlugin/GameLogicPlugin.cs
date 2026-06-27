using DragaliaAPI.Photon.Shared.Enums;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin;

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

        // C0 is in parameters[7] = 248
        this.roomState.QuestId = info.Request.GameProperties.GetInt(GamePropertyKeys.QuestId);
        this.roomState.IsRandomMatching = QuestHelper.GetIsRandomMatching(this.roomState.QuestId);
        MatchingTypes matchingType = (MatchingTypes)
            info.Request.GameProperties.GetInt(GamePropertyKeys.MatchingType);

        this.LogGameCreated(viewerId, this.PluginHost.GameId, roomId);

        this.pluginStateService.IsPubliclyVisible =
            matchingType == MatchingTypes.Anyone && !this.roomState.IsRandomMatching;
    }

#pragma warning disable CA5394 // Do not use insecure randomness. This value is not security-sensitive.
    private static int GenerateRoomId() => Random.Shared.Next(100_0000, 1_000_0000);
#pragma warning restore CA5394
}
