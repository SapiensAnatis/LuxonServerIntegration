using Luxon.Server.GluonPlugin.Shared;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;

namespace Luxon.Server.GluonPlugin.Plugins.Gluon;

public class GluonPluginFactory : IPluginFactory
{
    public GluonPluginFactory()
    {
        Console.WriteLine("GluonPluginFactory: ctor");
    }

    public IGamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(gameHost);

        Console.WriteLine("GluonPluginFactory: Create");

        var stateService = new PluginStateService();

        var configuration =
            config.Get<PluginConfiguration>()
            ?? throw new InvalidOperationException("Failed to get configuration");

        Console.WriteLine("GluonPluginFactory: ApiServerUrl=" + configuration.ApiServerUrl);

        var gameLogicPlugin = new GameLogic.GameLogicPlugin(gameHost, configuration, stateService);

        return new GluonPlugin(gameHost, stateService, gameLogicPlugin);
    }
}
