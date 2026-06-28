using System.Diagnostics;
using Luxon.Server;
using Luxon.Server.Host;
using Luxon.Server.Host.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

ILogger<Program> logger = LoggerConfiguration.Factory.CreateLogger<Program>();

if (Environment.GetEnvironmentVariable("LUXON_CUSTOM_LIB_PATH") is { } customLibPath)
{
    LogUsingCustomLibPath(logger, customLibPath);
    LuxonNativeLibrary.SetCustomLibraryPath(customLibPath);
}

string pluginPath = Path.Join(AppContext.BaseDirectory, "plugin.yml");

IConfigurationRoot configuration = new ConfigurationBuilder()
    .Add(new YamlConfigurationSource(pluginPath))
    .Build();

var pluginManager = new PluginManager(configuration);
pluginManager.LoadPluginsFromConfig();

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
    GamePluginOnJoinGame = csImports.GamePluginOnJoinGame,
    GamePluginOnCloseGame = csImports.GamePluginOnCloseGame,
    GamePluginOnRaiseEvent = csImports.GamePluginOnRaiseEvent,
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
            LogPluginTaskCompletedSuccessfully(logger);
        }
        else
        {
            if (task.IsCanceled)
            {
                LogPluginTaskWasCanceled(logger);
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

void OnUnhandledPluginException(Exception exception)
{
    Exception displayException =
        (exception is AggregateException { InnerExceptions: [{ } singleException] })
            ? singleException
            : exception;

    LogUnhandledExceptionInPluginCallback(logger, exception);
}

partial class Program
{
    [LoggerMessage(LogLevel.Error, "Unhandled exception in plugin callback")]
    static partial void LogUnhandledExceptionInPluginCallback(
        ILogger<Program> logger,
        Exception exception
    );

    [LoggerMessage(LogLevel.Debug, "Plugin task completed successfully")]
    static partial void LogPluginTaskCompletedSuccessfully(ILogger<Program> logger);

    [LoggerMessage(LogLevel.Information, "Using custom lib path: {CustomLibPath}")]
    static partial void LogUsingCustomLibPath(ILogger<Program> logger, string customLibPath);

    [LoggerMessage(LogLevel.Warning, "Plugin task was canceled")]
    static partial void LogPluginTaskWasCanceled(ILogger<Program> logger);
}
