using System.Reflection;
using Luxon.Server;
using Luxon.Server.Host;
using Luxon.Server.Plugin;

// var configPath = Path.Join(AppContext.BaseDirectory, "config.example.yml");
// using var serverManager = ServerManager.CreateFromFile(configPath);

var pluginManager = new PluginManager();

pluginManager.LoadPluginsFromConfig(Path.Join(AppContext.BaseDirectory, "plugin.yml"));

var gameTable = new Dictionary<nint, GameAdditionalFields>();

var imports = new LuxonServerImports
{
    GamePluginOnAttach = (
        (id, game) =>
        {
            if (!pluginManager.TryCreatePlugin(id, out GamePlugin? plugin))
            {
                throw new InvalidOperationException($"Failed to create plugin with ID {id}");
            }

            gameTable.Add(
                game.DangerousHandle,
                new GameAdditionalFields() { Plugins = { [id] = plugin } }
            );
        }
    ),
    GamePluginOnCreateGame = (
        (id, game, request, creator, join, exist) =>
        {
            if (
                !gameTable.TryGetValue(
                    game.DangerousHandle,
                    out GameAdditionalFields? additionalFields
                )
            )
            {
                throw new InvalidOperationException("Failed to get game metadata");
            }

            if (!additionalFields.Plugins.TryGetValue(id, out GamePlugin? plugin))
            {
                throw new InvalidOperationException($"Failed to get plugin {id}");
            }

            plugin.OnCreateGame(null);

            return LuxonGamePluginResult.Continue;
        }
    ),
    GamePluginBeforeJoin = null,
    GamePluginOnJoinGame = null,
    GamePluginOnLeave = null,
    GamePluginOnRaiseEvent = null,
    GamePluginBeforeSetProperties = null,
    GamePluginOnSetProperties = null,
    GamePluginBeforeCloseGame = null,
    GamePluginOnCloseGame = null,
    AuthPluginOnAuthenticate = null,
    CustomLogSink = null,
    HookpointMasterServerHandleOperationRequestJoinGame = null,
    HookpointMasterServerHandleOperationRequestCreateGame = (
        (
            (handler, id) =>
            {
                Console.WriteLine("Joinin");
                return true;
            }
        )
    ),
    HookpointHandlerBaseHandleENetCommandOnMessage = null,
    HookpointAppLoadAppSettings = null,
};

LuxonServerRuntime.SetImports(imports);

using var config = ServerManagerConfig
    .Create()
    .AddServer(LuxonServerType.MasterServer, port: 5055)
    .AddServer(LuxonServerType.GameServer, port: 5056)
    .AddEndpoint(LuxonServerType.MasterServer, LuxonServerProtocol.Udp, "196.168.0.65")
    .AddEndpoint(LuxonServerType.GameServer, LuxonServerProtocol.Udp, "196.168.0.65")
    .SetLimits(enableIpv6: false, maxConnections: 1000, maxGamePeers: 32, tickTimeBudget: 100);

using var serverManager = ServerManager.CreateFromConfig(config);

LuxonServerRuntime.RegisterHookpoints(serverManager);

serverManager.Run();
