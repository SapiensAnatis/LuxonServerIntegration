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

        var gameLogicPlugin = new GameLogic.GameLogicPlugin(gameHost, configuration, stateService);

        return new GluonPlugin(gameHost, stateService, gameLogicPlugin);
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
