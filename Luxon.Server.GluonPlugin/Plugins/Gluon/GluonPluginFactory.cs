using System.Net.Http.Headers;
using Luxon.Server.GluonPlugin.Plugins.GameLogic;
using Luxon.Server.GluonPlugin.Plugins.StateManager;
using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.GluonPlugin.Plugins.Gluon;

public partial class GluonPluginFactory : IPluginFactory
{
    public GluonPluginFactory()
    {
        Console.WriteLine("GluonPluginFactory: ctor");
    }

    public IGamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(gameHost);

        var stateService = new PluginStateService();

        var configuration =
            config.Get<PluginConfiguration>()
            ?? throw new InvalidOperationException("Failed to get configuration");

        var logger = gameHost.CreateLogger<GluonPluginFactory>();
        LogCreatingNewGluonPluginWithUrl(logger, configuration.ApiServerUrl);

        gameHost.HttpClient.BaseAddress = configuration.ApiServerUrl;
        gameHost.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            configuration.BearerToken
        );

        var gameLogicPlugin = new GameLogicPlugin(gameHost, configuration, stateService);
        var stateManagerPlugin = new StateManagerPlugin(gameHost, stateService, configuration);

        return new GluonPlugin(gameHost, stateService, gameLogicPlugin, stateManagerPlugin);
    }

    [LoggerMessage(
        LogLevel.Debug,
        "Creating new GluonPlugin instance with ApiServerUrl = {ApiServerUrl}"
    )]
    static partial void LogCreatingNewGluonPluginWithUrl(
        ILogger<GluonPluginFactory> logger,
        Uri apiServerUrl
    );
}
