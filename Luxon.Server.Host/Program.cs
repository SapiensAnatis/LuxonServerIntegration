using Luxon.Server;
using Luxon.Server.Host;
using Luxon.Server.Plugin;

var pluginManager = new PluginManager();

pluginManager.LoadPluginsFromConfig(Path.Join(AppContext.BaseDirectory, "plugin.yml"));

var gameTable = new Dictionary<nint, GameAdditionalFields>();

var imports = new LuxonServerImports
{
    GamePluginOnAttach = (id, game) =>
    {
        if (!pluginManager.TryCreatePlugin(id, out GamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to create plugin with ID {id}");
        }

        gameTable.Add(
            game.DangerousHandle,
            new GameAdditionalFields() { Plugins = { [id] = plugin } }
        );
    },
    GamePluginOnCreateGame = (id, game, request, creator, join, exist) =>
    {
        if (
            !gameTable.TryGetValue(game.DangerousHandle, out GameAdditionalFields? additionalFields)
        )
        {
            throw new InvalidOperationException("Failed to get game metadata");
        }

        if (!additionalFields.Plugins.TryGetValue(id, out GamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to get plugin {id}");
        }

        plugin.OnCreateGame();

        return LuxonGamePluginResult.Continue;
    },
    GamePluginOnCloseGame = (id, game, create) =>
    {
        gameTable.Remove(game.DangerousHandle);

        return LuxonGamePluginResult.Continue;
    },
};

LuxonServerRuntime.SetImports(imports);

// Reading the config from yaml doesn't appear to start any servers. I might be doing something wrong!

using var config = ServerManagerConfig.Create();

config
    .AddServer(LuxonServerType.MasterServer, port: 5055)
    .AddServer(LuxonServerType.GameServer, port: 5056)
    .AddEndpoint(LuxonServerType.MasterServer, LuxonServerProtocol.Udp, "192.168.0.65:5055")
    .AddEndpoint(LuxonServerType.GameServer, LuxonServerProtocol.Udp, "192.168.0.65:5056");

using var serverManager = ServerManager.CreateFromConfig(config);

serverManager.Run();
