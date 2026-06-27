using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Enums.DictKeyCodes;
using Luxon.Server.Host.Plugin;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host;

internal sealed class CSharpServerImports(
    ServerManager serverManager,
    PluginManager pluginManager,
    GameManager gameManager
)
{
    public void GamePluginOnAttach(uint pluginId, Game game)
    {
        if (!pluginManager.TryCreatePlugin(pluginId, game, out IGamePlugin? plugin))
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
        if (gameManager.CurrentCallbackContext.Value is { } previousContext)
        {
            // The command has been restarted because a suspended command returned a plugin result.
            return this.HandleCommandRestart(previousContext, request);
        }

        if (!gameManager.TryGetGamePlugin(game.DangerousHandle, pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to retrieve plugin with ID {pluginId}");
        }

        // TODO: this is not true for all games
        const int firstActorNr = 1;

        if (plugin is PluginBase { PluginHost: PluginHost concretePluginHost })
        {
            Console.WriteLine("GamePluginOnCreateGame: Updating PluginHost.GameActors");
            concretePluginHost.GameActorsInternal.Add(new Actor() { ActorNr = firstActorNr });
        }

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        if (!TryGetActorProperties(opr, out TypelessLuxonDictionaryProxy? actorProperties))
        {
            throw new InvalidOperationException("Failed to get actor properties");
        }

        if (!TryGetGameProperties(opr, out TypelessLuxonDictionaryProxy? gameProperties))
        {
            throw new InvalidOperationException("Failed to get game properties");
        }

        JoinGameRequest pluginRequest = new()
        {
            ActorNr = firstActorNr,

            ActorProperties = actorProperties,
            GameProperties = gameProperties,

            OperationCode = opr.OperationCode,
            Parameters = opr.Parameters,
        };

        CreateGameCallInfo info = new()
        {
            Request = pluginRequest,
            CreateIfNotExists = createIfNotExist,
            OperationRequest = opr,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnCreateGame(info, cts.Token);

        if (pluginCallback.IsCompletedSuccessfully)
        {
            // The plugin callback completed synchronously.

            Console.WriteLine("GamePluginOnCreateGame: Callback returned synchronously");

            return FinishCommand(info, request, managedRequest);
        }

        if (info.HasResult())
        {
            // The plugin returned info before suspending its Task, so we can finish the Luxon command. We will keep
            // an eye on the Task via CompletedTaskQueue though, to ensure no exceptions are swallowed.

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by the main loop.
            TaskContext ctx = new() { PluginTask = pluginCallback, CancellationTokenSource = cts };
#pragma warning restore CA2000

            this.SetupTaskCallbacks(ctx);

            return FinishCommand(info, request, managedRequest);
        }

        Console.WriteLine("GamePluginOnCreateGame: Plugin suspended without setting result");

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by re-entry logic at the top of the function.
        InfoCallbackContext infoCtx = new()
        {
            PluginTask = pluginCallback,
            CancellationTokenSource = cts,
            CallInfo = info,
            CommandRestarter = serverManager.TakeCommandRestarter(),
            ManagedMessage = managedRequest,
        };
#pragma warning restore CA2000

        this.SetupInfoCallbacks(infoCtx);

        Console.WriteLine("GamePluginOnCreateGame: Added task to queue, returning Continue");

        return LuxonGamePluginResult.Continue;
    }

    public LuxonGamePluginResult GamePluginOnJoinGame(
        uint pluginId,
        Game game,
        SerMessageRef request,
        GamePeer joiner
    )
    {
        if (gameManager.CurrentCallbackContext.Value is { } previousContext)
        {
            // The command has been restarted because a suspended command returned a plugin result.
            return this.HandleCommandRestart(previousContext, request);
        }

        if (!gameManager.TryGetGamePlugin(game.DangerousHandle, pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to retrieve plugin with ID {pluginId}");
        }

        if (plugin is PluginBase { PluginHost: PluginHost concretePluginHost })
        {
            Console.WriteLine("GamePluginOnJoinGame: Updating PluginHost.GameActors");
            concretePluginHost.GameActorsInternal.Add(new Actor() { ActorNr = joiner.ActorId });
        }

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        if (!TryGetActorProperties(opr, out TypelessLuxonDictionaryProxy? actorProperties))
        {
            throw new InvalidOperationException("Failed to get actor properties");
        }

        if (!TryGetGameProperties(opr, out TypelessLuxonDictionaryProxy? gameProperties))
        {
            throw new InvalidOperationException("Failed to get game properties");
        }

        JoinGameCallInfo info = new()
        {
            Request = new JoinGameRequest()
            {
                ActorProperties = actorProperties,
                GameProperties = gameProperties,
                ActorNr = joiner.ActorId,
                OperationCode = opr.OperationCode,
                Parameters = opr.Parameters,
            },
            ActorNr = joiner.ActorId,
            OperationRequest = opr,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnJoin(info, cts.Token);

        if (pluginCallback.IsCompletedSuccessfully)
        {
            // The plugin callback completed synchronously.

            Console.WriteLine("GamePluginOnJoinGame: Callback returned synchronously");

            return FinishCommand(info, request, managedRequest);
        }

        if (info.HasResult())
        {
            // The plugin returned info before suspending its Task, so we can finish the Luxon command. We will keep
            // an eye on the Task via CompletedTaskQueue though, to ensure no exceptions are swallowed.

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by the main loop.
            TaskContext ctx = new() { PluginTask = pluginCallback, CancellationTokenSource = cts };
#pragma warning restore CA2000

            this.SetupTaskCallbacks(ctx);

            return FinishCommand(info, request, managedRequest);
        }

        Console.WriteLine("GamePluginOnJoinGame: Plugin suspended without setting result");

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by re-entry logic at the top of the function.
        InfoCallbackContext infoCtx = new()
        {
            PluginTask = pluginCallback,
            CancellationTokenSource = cts,
            CallInfo = info,
            CommandRestarter = serverManager.TakeCommandRestarter(),
            ManagedMessage = managedRequest,
        };
#pragma warning restore CA2000

        this.SetupInfoCallbacks(infoCtx);

        Console.WriteLine("GamePluginOnJoinGame: Added task to queue, returning Continue");

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

        request.Replace(managedRequest);

        return result;
    }

    private LuxonGamePluginResult HandleCommandRestart(
        InfoCallbackContext previousContext,
        SerMessageRef request
    )
    {
        gameManager.CurrentCallbackContext.Value = null;

        if (previousContext.CallInfo is null)
        {
            throw new InvalidOperationException(
                "A CommandRestarter was invoked, but context.CallInfo was null"
            );
        }

        Console.WriteLine("GamePluginOnCreateGame: Detected previous context");

        var result = FinishCommand(
            previousContext.CallInfo,
            request,
            previousContext.ManagedMessage
        );

        // If the plugin has set a result, but is now suspended again, it will be monitored by the task queue; we added it when we originally added it to the info queue.
        // Do not dispose in this case.

        if (previousContext.PluginTask.IsCompleted)
        {
            previousContext.Dispose();
        }

        return result;
    }

    private void SetupTaskCallbacks(TaskContext ctx)
    {
        ctx.PluginTask.GetAwaiter().OnCompleted(() => gameManager.CompletedTaskQueue.Enqueue(ctx));
    }

    private void SetupInfoCallbacks(InfoCallbackContext ctx)
    {
        this.SetupTaskCallbacks(ctx);

        ctx.CallInfo.OnResultSet(() => gameManager.CommandRestartQueue.Enqueue(ctx));
    }

    private static bool TryGetActorProperties(
        OperationRequestMessage opr,
        [NotNullWhen(true)] out TypelessLuxonDictionaryProxy? proxy
    )
    {
        return TryGetHashtable(opr, (byte)Properties.ActorProperties, out proxy);
    }

    private static bool TryGetGameProperties(
        OperationRequestMessage opr,
        [NotNullWhen(true)] out TypelessLuxonDictionaryProxy? proxy
    )
    {
        return TryGetHashtable(opr, (byte)Properties.GameProperties, out proxy);
    }

    private static bool TryGetHashtable(
        OperationRequestMessage opr,
        byte key,
        out TypelessLuxonDictionaryProxy? proxy
    )
    {
        proxy = null;

        if (!opr.Parameters.TryGetValue(key, out Value hashtableValue))
        {
            return false;
        }

        if (hashtableValue is not { TypeTag: 21, Content: IDictionary<Value, Value> hashtable })
        {
            return false;
        }

        proxy = new TypelessLuxonDictionaryProxy(hashtable);

        return true;
    }
}
