using System.Text.Json;
using Luxon.Server.Plugin;
using MessagePack;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin;

internal sealed class GoToIngameStateManager
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
}
