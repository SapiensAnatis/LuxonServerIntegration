using System.Text.Json;
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

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
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

    public void OnSetGoToIngameState(IBeforeSetPropertiesCallInfo info, int value)
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
            this.OnMinStateChange(info);
        }
        else if (value == 1 && info.ActorNr == 1)
        {
            this.MinGoToIngameState = 1;
            this.OnMinStateChange(info);
        }
        else if (value == 0 && this.pluginHost.GetIsSoloPlay())
        {
            // For time attack solo quests that don't have a lobby phase
            this.SetGoToIngameInfo();
            this.pluginHost.RaiseEvent(Event.StartQuest, new Dictionary<string, object>());
        }
    }

    /// <summary>
    /// Handler to perform various operations when the minimum GoToIngameState of a room is increased.
    /// </summary>
    /// <remarks>
    /// Represents various stages of loading into a quest, during which events/properties need to be raised/set.
    /// </remarks>
    /// <param name="info">Call info.</param>
    private void OnMinStateChange(ICallInfo info)
    {
        this.LogUpdatingMinState(this.MinGoToIngameState);

        switch (this.MinGoToIngameState)
        {
            case 1:
                this.SetGoToIngameInfo();
                break;
            // case 2:
            //     this.RequestHeroParam(info);
            //     break;
            // case 3:
            //     this.RaisePartyEvent();
            //     break;
            // case 4:
            //     this.RaiseCharacterDataEvent();
            //     break;
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
}
