using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;

namespace Luxon.Server.GluonPlugin;

public class GluonPluginFactory : IPluginFactory
{
    public IGamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(gameHost);

        Console.WriteLine("GluonPlugin: Create");

        var stateService = new PluginStateService();

        var configuration =
            config.Get<PluginConfiguration>()
            ?? throw new InvalidOperationException("Failed to get configuration");

        var gameLogicPlugin = new GameLogicPlugin(gameHost, configuration, stateService);

        return new GluonPlugin(gameHost, stateService, gameLogicPlugin);
    }
}
