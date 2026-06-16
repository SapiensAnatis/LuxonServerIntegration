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
            _previousCallbackContext.Value = null;

            Console.WriteLine("GamePluginOnCreateGame: Detected previous context");

            Debug.Assert(
                previousContext.PluginCallback.IsCompleted,
                "The processing loop should only invoke the restarter once PluginCallback.IsCompleted is true"
            );

            return FinishCommand(previousContext.Info, request, previousContext.ManagedRequest);
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

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CreateGameCallInfo info = new(pluginRequest, createIfNotExist);
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnCreateGame(info);

        if (pluginCallback.IsCompleted || info.HasResult())
        {
            // The plugin callback completed synchronously, or returned a result before suspending. Either way, we do
            // not need to suspend the Luxon command.

            Console.WriteLine("GamePluginOnCreateGame: Callback returned status synchronously");

            return FinishCommand(info, request, managedRequest);
        }

        Console.WriteLine("GamePluginOnCreateGame: Plugin suspended without setting result");

        _previousCallbackContext.Value = new(pluginCallback, info, managedRequest);

        if (!gameManager.taskQueue.TryAdd(pluginCallback, serverManager.TakeCommandRestarter()))
        {
            throw new InvalidOperationException("Failed to add to task queue");
        }

        Console.WriteLine("GamePluginOnCreateGame: Added task to queue, returning Continue");

        return LuxonGamePluginResult.Continue;
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

    private static LuxonGamePluginResult FinishCommand(
        CallInfo info,
        SerMessageRef request,
        Message managedRequest
    )
    {
        if (!info.HasResult())
        {
            throw new InvalidOperationException(
                "Failed to set result before returning from plugin!"
            );
        }

        var result = info.Result.Value;

        info.Dispose();
        request.Replace(managedRequest);

        return result;
    }

    private record PreviousCallbackContext(
        Task PluginCallback,
        CallInfo Info,
        Message ManagedRequest
    );
}
