using Luxon.Server;
using Luxon.Server.Host;

var pluginManager = new PluginManager();
pluginManager.LoadPluginsFromConfig(Path.Join(AppContext.BaseDirectory, "plugin.yml"));

var gameManager = new GameManager();

var csImports = new CSharpServerImports(pluginManager, gameManager);

var imports = new LuxonServerImports
{
    GamePluginOnAttach = csImports.GamePluginOnAttach,
    GamePluginOnCreateGame = csImports.GamePluginOnCreateGame,
    GamePluginOnCloseGame = csImports.GamePluginOnCloseGame,
};

LuxonServerRuntime.SetImports(imports);

LuxonServerRuntime.UnhandledCallbackException += (sender) =>
{
    Console.WriteLine($"Unhandled callback exception: {sender}");
};

// Reading the config from YAML doesn't appear to start any servers. I might be doing something wrong!

using var config = ServerManagerConfig.Create();

config
    .AddServer(LuxonServerType.MasterServer, port: 5055)
    .AddServer(LuxonServerType.GameServer, port: 5056)
    .AddEndpoint(LuxonServerType.MasterServer, LuxonServerProtocol.Udp, "192.168.0.65:5055")
    .AddEndpoint(LuxonServerType.GameServer, LuxonServerProtocol.Udp, "192.168.0.65:5056");

using var serverManager = ServerManager.CreateFromConfig(config);

serverManager.Run();
