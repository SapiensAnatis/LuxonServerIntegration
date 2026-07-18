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
    .AddEnvironmentVariables()
    .Build();

var logImports = new LogImports();

LuxonServerRuntime.EnableCustomLogSink(true);
LuxonServerRuntime.SetImports(
    new LuxonServerImports() { CustomLogSink = logImports.CustomLogSink }
);

using var pluginManager = new PluginManager(configuration);
pluginManager.LoadPluginsFromConfig();

var gameManager = new GameManager();

var csImports = new CSharpServerImports(pluginManager, gameManager);

LuxonServerRuntime.SetImports(
    new LuxonServerImports
    {
        GamePluginOnAttach = csImports.GamePluginOnAttach,
        GamePluginOnCreateGame = csImports.GamePluginOnCreateGame,
        GamePluginOnJoinGame = csImports.GamePluginOnJoinGame,
        GamePluginOnCloseGame = csImports.GamePluginOnCloseGame,
        GamePluginOnRaiseEvent = csImports.GamePluginOnRaiseEvent,
        GamePluginBeforeSetProperties = csImports.GamePluginBeforeSetProperties,
        GamePluginOnSetProperties = csImports.GamePluginOnSetProperties,
        GamePluginOnLeave = csImports.GamePluginOnLeave,
        CustomLogSink = logImports.CustomLogSink,
    }
);

LuxonServerRuntime.UnhandledCallbackException += OnUnhandledPluginException;

using var serverManager = ServerManager.CreateFromFile(
    Path.Join(AppContext.BaseDirectory, "config.yml")
);

while (serverManager.RunOnce())
{
#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by the task queue monitor.
    if (gameManager.ResultSetQueue.TryDequeue(out var commandContext))
#pragma warning restore CA2000
    {
        LuxonGamePluginResult result = CSharpServerImports.FinishCommand(
            commandContext.CallInfo,
            commandContext.Request,
            commandContext.ManagedRequest
        );

        commandContext.CoroutineHandle.Resume(result);
    }

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
                int oldLength = gameManager.ResultSetQueue.Count;
#endif
                infoContext.CallInfo.Fail();

#if DEBUG
                Debug.Assert(
                    gameManager.ResultSetQueue.Count == oldLength + 1,
                    "Calling Fail should add the info to the queue"
                );
#endif
            }
        }

        taskContext.Dispose();
    }
}

void OnUnhandledPluginException(Exception exception)
{
    Exception displayException =
        (exception is AggregateException { InnerExceptions: [{ } singleException] })
            ? singleException
            : exception;

    LogUnhandledExceptionInPluginCallback(logger, displayException);
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
