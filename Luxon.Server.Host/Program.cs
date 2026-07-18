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

using var pluginManager = new PluginManager(configuration);
pluginManager.LoadPluginsFromConfig();

var gameManager = new GameManager();
var coroutineMonitor = new CoroutineMonitor();

var logImports = new LogImports();
var csImports = new CSharpServerImports(pluginManager, gameManager, coroutineMonitor);

LuxonServerRuntime.EnableCustomLogSink(true);

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

LuxonServerRuntime.UnhandledCallbackException += coroutineMonitor.OnUnhandledPluginException;

using var serverManager = ServerManager.CreateFromFile(
    Path.Join(AppContext.BaseDirectory, "config.yml")
);

while (serverManager.RunOnce())
{
    coroutineMonitor.ServiceCompletedCallInfos();
    coroutineMonitor.ServiceCompletedTasks();
}

partial class Program
{
    [LoggerMessage(LogLevel.Information, "Using custom lib path: {CustomLibPath}")]
    static partial void LogUsingCustomLibPath(ILogger<Program> logger, string customLibPath);
}
