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

        IGamePlugin plugin = this.GetPlugin(game, pluginId);

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        // TODO: this is not true for all games
        const int firstActorNr = 1;

        CreateGameCallInfo info = new()
        {
            Request = CreateJoinGameRequest(opr, firstActorNr),
            CreateIfNotExists = createIfNotExist,
            OperationRequest = opr,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnCreateGame(info, cts.Token);

        return this.HandlePluginTaskResult(pluginCallback, info, request, managedRequest, cts);
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

        IGamePlugin plugin = this.GetPlugin(game, pluginId);

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

        JoinGameCallInfo info = new()
        {
            Request = CreateJoinGameRequest(opr, joiner.ActorId),
            ActorNr = joiner.ActorId,
            OperationRequest = opr,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnJoin(info, cts.Token);

        return this.HandlePluginTaskResult(pluginCallback, info, request, managedRequest, cts);
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

    private LuxonGamePluginResult HandlePluginTaskResult(
        Task pluginCallback,
        CallInfo info,
        SerMessageRef request,
        Message managedRequest,
        CancellationTokenSource cts
    )
    {
        if (pluginCallback.IsCompletedSuccessfully)
        {
            // The plugin callback completed synchronously.

            Console.WriteLine("CSharpServerImports: Callback returned synchronously");

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

        Console.WriteLine("CSharpServerImports: Plugin suspended without setting result");

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by re-entry logic.
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

        Console.WriteLine("CSharpServerImports: Added task to queue, returning Continue");

        return LuxonGamePluginResult.Continue;
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

    private IGamePlugin GetPlugin(Game game, uint pluginId)
    {
        if (!gameManager.TryGetGamePlugin(game.DangerousHandle, pluginId, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to retrieve plugin with ID {pluginId}");
        }

        return plugin;
    }

    private static JoinGameRequest CreateJoinGameRequest(OperationRequestMessage msg, int actorNr)
    {
        if (
            !TryGetHashtable(
                msg,
                Properties.ActorProperties,
                out TypelessLuxonDictionaryProxy? actorProperties
            )
        )
        {
            throw new InvalidOperationException("Failed to get actor properties");
        }

        if (
            !TryGetHashtable(
                msg,
                Properties.GameProperties,
                out TypelessLuxonDictionaryProxy? gameProperties
            )
        )
        {
            throw new InvalidOperationException("Failed to get game properties");
        }

        return new JoinGameRequest()
        {
            ActorProperties = actorProperties,
            GameProperties = gameProperties,
            ActorNr = actorNr,
            OperationCode = msg.OperationCode,
            Parameters = msg.Parameters,
        };
    }

    private static bool TryGetHashtable(
        OperationRequestMessage opr,
        Properties key,
        [MaybeNullWhen(false)] out TypelessLuxonDictionaryProxy proxy
    )
    {
        proxy = null;

        if (!opr.Parameters.TryGetValue((byte)key, out Value hashtableValue))
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
