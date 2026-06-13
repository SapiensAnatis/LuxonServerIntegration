using System.Reflection;
using Luxon.Server;
using Luxon.Server.Host;
using Luxon.Server.Plugin;

var configPath = Path.Join(AppContext.BaseDirectory, "config.example.yml");

using var serverManager = ServerManager.CreateFromFile(configPath);

var pluginManager = new PluginManager();

pluginManager.LoadPluginsFromConfig(Path.Join(AppContext.BaseDirectory, "plugin.yml"));

var gameTable = new Dictionary<nint, GameAdditionalFields>();

LuxonServerRuntime.SetImports(
    new LuxonServerImports
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
        HookpointMasterServerHandleOperationRequestCreateGame = null,
        HookpointHandlerBaseHandleENetCommandOnMessage = null,
        HookpointAppLoadAppSettings = null,
    }
);

serverManager.Run();
