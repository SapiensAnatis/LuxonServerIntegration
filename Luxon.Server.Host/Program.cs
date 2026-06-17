using System.Diagnostics;
using Luxon.Server;
using Luxon.Server.Host;

if (Environment.GetEnvironmentVariable("LUXON_CUSTOM_LIB_PATH") is { } customLibPath)
{
    Console.WriteLine($"[ServerHost] [debug] Using custom lib path: {customLibPath}");
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
    if (gameManager.CompletedTaskQueue.TryDequeue(out var taskContext))
    {
        var task = taskContext.PluginTask;

        Debug.Assert(
            task.IsCompleted,
            "The only thing that adds to CompletedTaskQueue is the OnCompleted callback of task awaiters"
        );

        if (task.IsCompletedSuccessfully)
        {
            // TODO: This logs twice even if there is only one callback being called...

            Console.WriteLine("[ServerHost] [debug] Plugin task completed successfully");
        }
        else
        {
            if (task.IsCanceled)
            {
                Console.WriteLine("[ServerHost] [warning] Plugin task was cancelled");
            }
            else if (task.IsFaulted)
            {
                OnUnhandledPluginException(task.Exception);
            }

            if (taskContext is InfoCallbackContext infoContext && !infoContext.CallInfo.HasResult())
            {
                // If the task was cancelled or failed before setting a result, then set an error and restart the command.
                // This will enqueue it in CommandRestartQueue.
#if DEBUG
                int oldLength = gameManager.CommandRestartQueue.Count;
#endif
                infoContext.CallInfo.Fail();

#if DEBUG
                Debug.Assert(
                    gameManager.CommandRestartQueue.Count == oldLength + 1,
                    "Calling Fail should add the info to the queue"
                );
#endif
            }
        }

        taskContext.Dispose();
    }

    if (gameManager.CommandRestartQueue.TryDequeue(out var commandContext))
    {
        gameManager.CurrentCallbackContext.Value = commandContext;
        commandContext.CommandRestarter.Restart();
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
