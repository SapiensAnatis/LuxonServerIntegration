using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Enums.DictKeyCodes;
using Luxon.Server.Host.Plugin;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host;

[SuppressMessage(
    "Design",
    "CA1001:Types that own disposable fields should be disposable",
    Justification = "Object lives for entire process lifetime"
)]
internal sealed class CSharpServerImports(
    ServerManager serverManager,
    PluginManager pluginManager,
    GameManager gameManager
)
{
    private readonly ThreadLocal<PreviousCallbackContext?> _previousCallbackContext = new();

    public void GamePluginOnAttach(uint pluginId, Game game)
    {
        if (!pluginManager.TryCreatePlugin(pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to create plugin with ID {pluginId}");
        }

        gameManager.AddPluginToGame(game.DangerousHandle, pluginId, plugin);
    }

    public LuxonGamePluginResult GamePluginOnCreateGame(
        uint pluginId,
        Game game,
        SerMessageRef request,
        Peer creator,
        bool isJoin,
        bool createIfNotExist
    )
    {
        if (_previousCallbackContext.Value is { } previousContext)
        {
            Console.WriteLine("GamePluginOnCreateGame: Detected previous context");

            Debug.Assert(
                previousContext.PluginCallback.IsCompleted,
                "The processing loop should only invoke the restarter once PluginCallback.IsCompleted is true"
            );

            if (!previousContext.Info.HasResult())
            {
                throw new InvalidOperationException(
                    "Failed to set result before returning from plugin!"
                );
            }

            request.Replace(previousContext.ManagedRequest);

            var value = previousContext.Info.Result.Value;

            _previousCallbackContext.Value = null;
            previousContext.Info.Dispose();
            return value;
        }

        if (!gameManager.TryGetGamePlugin(game.DangerousHandle, pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to retrieve plugin with ID {pluginId}");
        }

        Message managedRequest = request.ToManagedMessage();

        if (
            managedRequest.Payload is not OperationRequestMessage opr
            || !opr.Parameters.TryGetValue(
                (byte)Properties.ActorProperties,
                out Value actorProperties
            )
        )
        {
            throw new InvalidOperationException("Unexpected message type");
        }

        if (
            actorProperties
            is not { TypeTag: 21, Content: IDictionary<Value, Value> actorPropertiesDictionary }
        )
        {
            throw new InvalidOperationException("Unexpected actor properties type");
        }

        var proxy = new TypelessLuxonDictionaryProxy(actorPropertiesDictionary);

        JoinGameRequest pluginRequest = new()
        {
            ActorProperties = proxy,
            ActorNr = 1,
            OperationCode = opr.OperationCode,
            Parameters = opr.Parameters,
        };

        CreateGameCallInfo info = new(pluginRequest, createIfNotExist);

        Task pluginCallback = plugin.OnCreateGame(info);

        if (pluginCallback.IsCompleted)
        {
            // The plugin callback completed synchronously. Perform standard checks.

            Console.WriteLine("GamePluginOnCreateGame: Callback returned synchronously");

            if (!info.HasResult())
            {
                throw new InvalidOperationException(
                    "Failed to set result before returning from plugin!"
                );
            }

            goto HasResult;
        }

        // If we are here, the plugin callback has suspended due to the use of await.

        if (info.HasResult())
        {
            // If the plugin callback set a result before awaiting, we can consume that without restarting the command.

            Console.WriteLine(
                "GamePluginOnCreateGame: Plugin suspended, but set result before doing so"
            );

            goto HasResult;
        }

        Console.WriteLine("GamePluginOnCreateGame: Plugin suspended without setting result");

        _previousCallbackContext.Value = new(pluginCallback, info, managedRequest);

        if (!gameManager.taskQueue.TryAdd(pluginCallback, serverManager.TakeCommandRestarter()))
        {
            throw new InvalidOperationException("Failed to add to task queue");
        }

        Console.WriteLine("GamePluginOnCreateGame: Added task to queue, returning Continue");

        return LuxonGamePluginResult.Continue;

        HasResult:

        // We cannot call InsertActorProperties at this stage as there is no GamePeer
        request.Replace(managedRequest);

        var infoValue = info.Result.Value;
        info.Dispose();
        return infoValue;
    }

    public LuxonGamePluginResult GamePluginOnCloseGame(
        uint pluginId,
        Game game,
        bool failedOnCreate
    )
    {
        // TODO: this doesn't fire for disconnects due to errors

        gameManager.RemovePlugin(game.DangerousHandle, pluginId);

        return LuxonGamePluginResult.Continue;
    }

    private record PreviousCallbackContext(
        Task PluginCallback,
        CallInfo Info,
        Message ManagedRequest
    );
}
