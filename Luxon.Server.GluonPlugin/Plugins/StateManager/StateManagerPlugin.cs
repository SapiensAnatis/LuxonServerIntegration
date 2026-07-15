using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DragaliaAPI.Photon.Shared.Enums;
using DragaliaAPI.Photon.Shared.Models;
using DragaliaAPI.Photon.Shared.Requests;
using Luxon.Server.GluonPlugin.Plugins.GameLogic.Events;
using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.GluonPlugin.Shared.Constants;
using Luxon.Server.GluonPlugin.Shared.Helpers;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.StateManager;

internal sealed partial class StateManagerPlugin : PluginBase
{
    private static readonly Uri GameCreateEndpoint = new("Event/GameCreate", UriKind.Relative);

    private static readonly Uri GameCloseEndpoint = new("Event/GameClose", UriKind.Relative);

    private static readonly Uri GameJoinEndpoint = new("Event/GameJoin", UriKind.Relative);

    private static readonly Uri GameLeaveEndpoint = new("Event/GameLeave", UriKind.Relative);

    private static readonly Uri EntryConditionsEndpoint = new(
        "Event/EntryConditions",
        UriKind.Relative
    );

    private static readonly Uri MatchingTypeEndpoint = new("Event/MatchingType", UriKind.Relative);

    private static readonly Uri RoomIdEndpoint = new("Event/RoomId", UriKind.Relative);

    private static readonly Uri VisibleEndpoint = new("Event/Visible", UriKind.Relative);

    private static readonly JsonSerializerOptions JsonSerializerSettings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly PluginStateService pluginStateService;
    private readonly PluginConfiguration configuration;

    private ILogger<StateManagerPlugin> logger;
    private bool roomHidden;

    private Uri StateManagerUrl
    {
        get { return this.configuration.StateManagerUrl; }
    }

    private string BearerToken
    {
        get { return this.configuration.BearerToken; }
    }

    public StateManagerPlugin(
        IPluginHost pluginHost,
        PluginStateService pluginStateService,
        PluginConfiguration configuration
    )
        : base(pluginHost)
    {
        this.pluginStateService = pluginStateService;
        this.configuration = configuration;

        this.logger = pluginHost.CreateLogger<StateManagerPlugin>();
    }

    public override async Task OnCreateGame(
        ICreateGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
        // https://doc.photonengine.com/server/current/plugins/plugins-faq#how_to_get_the_actor_number_in_plugin_callbacks_
        int actorNr = 1;

        using HttpRequestMessage request = this.CreatePostRequest(
            GameCreateEndpoint,
            new GameCreateRequest()
            {
                Game = DtoHelpers.CreateGame(this.PluginHost.GameId, info.Request.GameProperties),
                Player = DtoHelpers.CreatePlayer(actorNr, info.Request.ActorProperties),
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnCreateGame", res.StatusCode);
        }
    }

    public override async Task OnJoin(IJoinGameCallInfo info, CancellationToken cancellationToken)
    {
        if (info.ActorNr == 1)
        {
            // Already added as part of OnCreateGame
            return;
        }

        using HttpRequestMessage request = this.CreatePostRequest(
            GameJoinEndpoint,
            new GameModifyRequest
            {
                GameName = this.PluginHost.GameId,
                Player = DtoHelpers.CreatePlayer(info.ActorNr, info.Request.ActorProperties),
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnJoin", res.StatusCode);
        }
    }

    public override async Task OnLeave(ILeaveGameCallInfo info, CancellationToken cancellationToken)
    {
        if (info.ActorNr == -1)
        {
            // Actor was never really in the room, server disconnect, etc.
            return;
        }

        IActor? actor = this.PluginHost.GameActors.FirstOrDefault(x => x.ActorNr == info.ActorNr);

        if (actor is null)
        {
            this.LogCouldNotFindActor(info.ActorNr);
            return;
        }

        using HttpRequestMessage request = this.CreatePostRequest(
            GameLeaveEndpoint,
            new GameModifyRequest
            {
                GameName = this.PluginHost.GameId,
                Player = DtoHelpers.CreatePlayer(info.ActorNr, actor.Properties),
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnLeave", res.StatusCode);
        }
    }

    public override async Task OnCloseGame(
        ICloseGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
        using HttpRequestMessage request = this.CreatePostRequest(
            GameCloseEndpoint,
            new GameModifyRequest { GameName = this.PluginHost.GameId, Player = null }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnCloseGame", res.StatusCode);
        }
    }

    public override async Task OnSetProperties(
        ISetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
        if (info.Request.Properties.ContainsKey(GamePropertyKeys.EntryConditions))
        {
            await this.OnSetEntryConditions(info, cancellationToken);
        }

        if (info.Request.Properties.ContainsKey(GamePropertyKeys.MatchingType))
        {
            await this.OnSetMatchingType(info, cancellationToken);
        }

        if (info.Request.Properties.ContainsKey(ActorPropertyKeys.GoToIngameState))
        {
            await this.OnSetGoToIngameState(cancellationToken);
        }
    }

    public override async Task OnRaiseEvent(
        IRaiseEventCallInfo info,
        CancellationToken cancellationToken
    )
    {
        if (info.Request.EvCode == (int)Event.GameSucceed && info.ActorNr == 1)
        {
            await this.OnGameSucceed(cancellationToken);
            await this.OnSetRoomId(cancellationToken);
        }
    }

    private async Task OnGameSucceed(CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = this.CreatePostRequest(
            VisibleEndpoint,
            new GameModifyVisibleRequest
            {
                NewVisibility = true,
                GameName = this.PluginHost.GameId,
                Player = null,
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("GameSucceed", res.StatusCode);
        }

        this.roomHidden = false;
    }

    private async Task OnSetMatchingType(
        ISetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
        MatchingTypes newType = (MatchingTypes)
            info.Request.Properties.GetInt(GamePropertyKeys.MatchingType);

        using HttpRequestMessage request = this.CreatePostRequest(
            MatchingTypeEndpoint,
            new GameModifyMatchingTypeRequest()
            {
                GameName = this.PluginHost.GameId,
                NewMatchingType = newType,
                Player = null,
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnSetMatchingType", res.StatusCode);
        }
    }

    private async Task OnSetEntryConditions(
        ISetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
        EntryConditions? newEntryConditions = DtoHelpers.CreateEntryConditions(
            info.Request.Properties
        );

        if (newEntryConditions is null)
        {
            return;
        }

        using HttpRequestMessage request = this.CreatePostRequest(
            EntryConditionsEndpoint,
            new GameModifyConditionsRequest()
            {
                GameName = this.PluginHost.GameId,
                NewEntryConditions = newEntryConditions,
                Player = null,
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnSetEntryConditions", res.StatusCode);
        }
    }

    private async Task OnSetGoToIngameState(CancellationToken cancellationToken)
    {
        IEnumerable<int> goToIngameState = this.PluginHost.GameActors.Select(x =>
            x.Properties.GetIntOrDefault(ActorPropertyKeys.GoToIngameState)
        );

        bool shouldHideRoom = goToIngameState.All(x => x > 1) && !this.roomHidden;

        if (!shouldHideRoom)
        {
            return;
        }

        using HttpRequestMessage request = this.CreatePostRequest(
            VisibleEndpoint,
            new GameModifyVisibleRequest
            {
                NewVisibility = false,
                GameName = this.PluginHost.GameId,
                Player = null,
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnSetGoToIngameState", res.StatusCode);
        }

        this.roomHidden = true;
    }

    private async Task OnSetRoomId(CancellationToken cancellationToken)
    {
        int roomId = this.PluginHost.GameProperties.GetInt(GamePropertyKeys.RoomId);

        using HttpRequestMessage request = this.CreatePostRequest(
            RoomIdEndpoint,
            new GameModifyRoomIdRequest
            {
                NewRoomId = roomId,
                GameName = this.PluginHost.GameId,
                Player = null,
            }
        );

        var res = await this.PluginHost.HttpClient.SendAsync(request, cancellationToken);

        if (!res.IsSuccessStatusCode)
        {
            this.LogStateManagerRequestFailed("OnSetRoomId", res.StatusCode);
        }
    }

    private HttpRequestMessage CreatePostRequest(Uri endpoint, object request)
    {
        Uri requestUri = new(this.StateManagerUrl, endpoint);

        JsonContent content = JsonContent.Create(request, options: JsonSerializerSettings);

        return new HttpRequestMessage()
        {
            RequestUri = requestUri,
            Content = content,
            Method = HttpMethod.Post,
        };
    }

    [LoggerMessage(
        LogLevel.Warning,
        "OnLeave: could not find actor {ActorNr} -- GameLeave request aborted"
    )]
    partial void LogCouldNotFindActor(int actorNr);

    [LoggerMessage(
        LogLevel.Error,
        "{Method}: StateManager request failed with status {StatusCode}"
    )]
    partial void LogStateManagerRequestFailed(string method, HttpStatusCode statusCode);
}
