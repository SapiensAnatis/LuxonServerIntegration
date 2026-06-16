using System.Runtime.InteropServices;
using Luxon.Server;
using Luxon.Server.Host;

if (Environment.GetEnvironmentVariable("LUXON_CUSTOM_LIB_PATH") is { } customLibPath)
{
    Console.WriteLine($"[ServerManager] [debug] Using custom lib path: {customLibPath}");
    LuxonNativeLibrary.SetCustomLibraryPath(customLibPath);
}

var pluginManager = new PluginManager();
pluginManager.LoadPluginsFromConfig(Path.Join(AppContext.BaseDirectory, "plugin.yml"));

var gameManager = new GameManager();

using var config = ServerManagerConfig.Create();

config
    .AddServerAndEndpoint(LuxonServerType.MasterServer, 5055, "192.168.0.65:5055")
    .AddServerAndEndpoint(LuxonServerType.GameServer, 5056, "192.168.0.65:5056");
using var serverManager = ServerManager.CreateFromConfig(config);

var csImports = new CSharpServerImports(serverManager, pluginManager, gameManager);

var imports = new LuxonServerImports
{
    GamePluginOnAttach = csImports.GamePluginOnAttach,
    GamePluginOnCreateGame = csImports.GamePluginOnCreateGame,
    GamePluginOnCloseGame = csImports.GamePluginOnCloseGame,
};

LuxonServerRuntime.SetImports(imports);

LuxonServerRuntime.UnhandledCallbackException += OnUnhandledPluginException;

// Reading the config from YAML doesn't appear to start any servers. I might be doing something wrong!

while (serverManager.RunOnce())
{
    foreach (var task in gameManager.taskQueue.Keys)
    {
        if (!task.IsCompleted)
        {
            continue;
        }
#pragma warning disable CA2000 // Dispose objects before losing scope. Restart() destroys the native object, thus an explicit dispose would be a double-free.
        if (!gameManager.taskQueue.TryRemove(task, out CommandRestarter? restarter))
#pragma warning restore CA2000
        {
            Console.WriteLine("[ServerManager] [warning] Failed to remove task from pool");
        }

        if (task.IsCompletedSuccessfully)
        {
            restarter?.Restart();
        }
        else if (task.IsCanceled)
        {
            // TODO: Wire this up to return LuxonPluginResult.Cancelled if we have a restarter, somehow?
            OnPluginCallbackCancelled();
        }
        else if (task.IsFaulted)
        {
            OnUnhandledPluginException(task.Exception);
        }
    }
}

static void OnUnhandledPluginException(Exception exception)
{
    Exception displayException =
        (exception is AggregateException { InnerExceptions: [{ } singleException] })
            ? singleException
            : exception;

    Console.WriteLine(
        $"[ServerHost] [error] Unhandled exception inside plugin callback: {displayException}"
    );
}

static void OnPluginCallbackCancelled()
{
    Console.WriteLine("[ServerHost] [warning] Plugin callback was cancelled");
}
