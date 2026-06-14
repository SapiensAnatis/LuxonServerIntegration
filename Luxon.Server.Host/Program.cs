using System.Runtime.InteropServices;
using Luxon.Server;
using Luxon.Server.Host;

if (Environment.GetEnvironmentVariable("LUXON_CUSTOM_LIB_PATH") is { } customLibPath)
{
    Console.WriteLine($"Using custom lib path: {customLibPath}");
    LuxonNativeLibrary.SetCustomLibraryPath(customLibPath);
}

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
    .AddServerAndEndpoint(LuxonServerType.MasterServer, 5055, "192.168.0.65:5055")
    .AddServerAndEndpoint(LuxonServerType.GameServer, 5056, "192.168.0.65:5056");

using var serverManager = ServerManager.CreateFromConfig(config);

serverManager.Run();
