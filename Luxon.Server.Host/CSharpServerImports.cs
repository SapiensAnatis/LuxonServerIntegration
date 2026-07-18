using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Enums.DictKeyCodes;
using Luxon.Server.Host.Plugin;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host;

internal sealed partial class CSharpServerImports
{
    private readonly PluginManager pluginManager;
    private readonly GameManager gameManager;
    private readonly CoroutineMonitor coroutineMonitor;

    [SuppressMessage(
        "Microsoft.Performance",
        "CA1823:AvoidUnusedPrivateFields",
        Justification = "Used by the logging source generator"
    )]
    private readonly ILogger<CSharpServerImports> logger;

    public CSharpServerImports(
        PluginManager pluginManager,
        GameManager gameManager,
        CoroutineMonitor coroutineMonitor
    )
    {
        this.pluginManager = pluginManager;
        this.gameManager = gameManager;
        this.coroutineMonitor = coroutineMonitor;

        this.logger = LoggerConfiguration.Factory.CreateLogger<CSharpServerImports>();
    }

    public void GamePluginOnAttach(uint pluginId, Game game)
    {
        if (!this.pluginManager.TryCreatePlugin(pluginId, game, out IGamePlugin? plugin))
        {
            throw new InvalidOperationException($"Failed to create plugin with ID {pluginId}");
        }

        this.gameManager.AddPluginToGame(game.DangerousHandle, pluginId, plugin);
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

        if (plugin is PluginBase { PluginHost: PluginHost concretePluginHost })
        {
            concretePluginHost.GameProperties = info.Request.GameProperties;
        }

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
        this.LogJoinGameCalled();

        IGamePlugin plugin = this.GetPlugin(game, pluginId);

        this.LogUpdatingGameActors(joiner.ActorId);

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

    public LuxonGamePluginResult GamePluginOnRaiseEvent(
        uint pluginId,
        Game game,
        SerMessageRef request,
        GamePeer raiser,
        LuxonEvent evt,
        byte cacheOp
    )
    {
        this.LogEventRaised(evt.RoutingMetadata.Code);

        IGamePlugin plugin = this.GetPlugin(game, pluginId);

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        var info = new RaiseEventCallInfo
        {
            OperationRequest = opr,
            Request = new RaiseEventRequest
            {
                EvCode = evt.RoutingMetadata.Code,
                OperationCode = opr.OperationCode,
                Parameters = opr.Parameters,
                Data = evt.Data.Content,
            },
            ActorNr = raiser.ActorId,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnRaiseEvent(info, cts.Token);

        return this.HandlePluginTaskResult(pluginCallback, info, request, managedRequest, cts);
    }

    public LuxonGamePluginResult GamePluginOnCloseGame(
        uint pluginId,
        Game game,
        bool failedOnCreate
    )
    {
        IGamePlugin plugin = this.GetPlugin(game, pluginId);

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        var info = new CloseGameCallInfo { OperationRequest = null! };

        Task pluginCallback = plugin.OnCloseGame(info, cts.Token);

        if (!info.IsProcessed)
        {
            throw new NotSupportedException(
                "OnCloseGame callbacks must return a plugin result synchronously"
            );
        }

        // They must return synchronously because I am not smart enough to make the below happen on resume
        this.gameManager.RemovePlugin(game.DangerousHandle, pluginId);

        return this.HandlePluginTaskResult(pluginCallback, info, null, null, cts);
    }

    public LuxonGamePluginResult GamePluginBeforeSetProperties(
        uint pluginId,
        Game game,
        SerMessageRef request,
        GamePeer setter,
        bool broadcast,
        int targetActorId,
        SerValueRef update,
        SerValueRef expected
    )
    {
        IGamePlugin plugin = this.GetPlugin(game, pluginId);

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        if (update.ToManagedValue() is not { Content: IDictionary<Value, Value> valueHashtable })
        {
            throw new InvalidOperationException("Failed to get properties hashtable");
        }

        var info = new BeforeSetPropertiesCallInfo()
        {
            OperationRequest = opr,
            Request = new SetPropertiesRequest()
            {
                Properties = new TypelessLuxonDictionaryProxy(valueHashtable),
                ActorNumber = targetActorId,
            },
            ActorNr = setter.ActorId,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.BeforeSetProperties(info, cts.Token);

        return this.HandlePluginTaskResult(pluginCallback, info, request, managedRequest, cts);
    }

    public LuxonGamePluginResult GamePluginOnSetProperties(
        uint pluginId,
        Game game,
        SerMessageRef request,
        GamePeer setter,
        bool broadcast,
        int targetActorId,
        SerValueRef update,
        SerValueRef expected
    )
    {
        IGamePlugin plugin = this.GetPlugin(game, pluginId);

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        if (update.ToManagedValue() is not { Content: IDictionary<Value, Value> valueHashtable })
        {
            throw new InvalidOperationException("Failed to get properties hashtable");
        }

        var info = new BeforeSetPropertiesCallInfo()
        {
            OperationRequest = opr,
            Request = new SetPropertiesRequest()
            {
                Properties = new TypelessLuxonDictionaryProxy(valueHashtable),
                ActorNumber = targetActorId,
            },
            ActorNr = setter.ActorId,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.BeforeSetProperties(info, cts.Token);

        return this.HandlePluginTaskResult(pluginCallback, info, request, managedRequest, cts);
    }

    public LuxonGamePluginResult GamePluginOnLeave(
        uint pluginId,
        Game game,
        SerMessageRef request,
        GamePeer leaver
    )
    {
        IGamePlugin plugin = this.GetPlugin(game, pluginId);

        Message managedRequest = request.ToManagedMessage();

        if (managedRequest.Payload is not OperationRequestMessage opr)
        {
            throw new InvalidOperationException("Failed to get OperationRequestMessage");
        }

        var info = new LeaveGameCallInfo()
        {
            OperationRequest = opr,
            Request = new LeaveGameRequest()
            {
                Parameters = opr.Parameters,
                OperationCode = opr.OperationCode,
            },
            Reason = 0,
            ActorNr = leaver.ActorId,
        };

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by FinishCommand, either synchronously or upon re-entry.
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
#pragma warning restore CA2000

        Task pluginCallback = plugin.OnLeave(info, cts.Token);

        return this.HandlePluginTaskResult(pluginCallback, info, request, managedRequest, cts);
    }

    public static LuxonGamePluginResult FinishCommand(
        CallInfo info,
        SerMessageRef? request,
        Message? managedRequest
    )
    {
        if (!info.HasResult())
        {
            throw new InvalidOperationException(
                "Failed to set result before returning from plugin!"
            );
        }

        var result = info.Result.Value;

        if (request is not null && managedRequest is not null)
        {
            request.Replace(managedRequest);
        }

        return result;
    }

    private LuxonGamePluginResult HandlePluginTaskResult(
        Task pluginCallback,
        CallInfo info,
        SerMessageRef? request,
        Message? managedRequest,
        CancellationTokenSource cts
    )
    {
        if (pluginCallback.IsCompletedSuccessfully)
        {
            // The plugin callback completed synchronously (or close enough to it).

            this.LogPluginCallbackReturnedSynchronously(info.GetType().Name, info.Result);

            cts.Dispose();

            return FinishCommand(info, request, managedRequest);
        }

        if (pluginCallback.IsFaulted)
        {
            // The task faulted synchronously.
            // Use ExceptionDispatchInfo so that CSharpServerImports does not appear in the stack trace.
            ExceptionDispatchInfo
                .Capture(pluginCallback.Exception.InnerException ?? pluginCallback.Exception)
                .Throw();
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

        this.LogPluginSuspendedWithoutSettingResult();

        // This will suspend the active command until we call Restart on it
        CoroutineHandle coroutineHandle = LuxonServerRuntime.GetActiveCoroutine();

#pragma warning disable CA2000 // Dispose objects before losing scope. Will be disposed by re-entry logic.
        InfoCallbackContext infoCtx = new()
        {
            PluginTask = pluginCallback,
            CancellationTokenSource = cts,
            CallInfo = info,
            CoroutineHandle = coroutineHandle,
            ManagedRequest = managedRequest,
            Request = request,
        };
#pragma warning restore CA2000

        this.SetupInfoCallbacks(infoCtx);

        this.LogSuspendedCommand();

        return LuxonGamePluginResult.Async;
    }

    private void SetupTaskCallbacks(TaskContext ctx)
    {
        ctx.PluginTask.GetAwaiter()
            .OnCompleted(() => this.coroutineMonitor.NotifyPluginTaskCompleted(ctx));
    }

    private void SetupInfoCallbacks(InfoCallbackContext ctx)
    {
        this.SetupTaskCallbacks(ctx);

        ctx.CallInfo.OnResultSet(() => this.coroutineMonitor.NotifyCallInfoResultSet(ctx));
    }

    private IGamePlugin GetPlugin(Game game, uint pluginId)
    {
        if (
            !this.gameManager.TryGetGamePlugin(
                game.DangerousHandle,
                pluginId,
                out IGamePlugin? plugin
            )
        )
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
            // When a user joins a game which they did not create, we won't get GameProperties, so substitute an empty
            // dictionary.
            // TODO: This branch should throw in the create game case, maybe check request.OperationCode?

            gameProperties = new TypelessLuxonDictionaryProxy(new Dictionary<Value, Value>());
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

    [LoggerMessage(LogLevel.Debug, "GamePluginOnJoinGame called")]
    partial void LogJoinGameCalled();

    [LoggerMessage(LogLevel.Debug, "Updating GameActors with actor number {ActorNr}")]
    partial void LogUpdatingGameActors(int actorNr);

    [LoggerMessage(LogLevel.Debug, "Detected previous command context")]
    partial void DetectedPreviousContext();

    [LoggerMessage(
        LogLevel.Trace,
        "Plugin callback set result ({InfoTypeName}) synchronously: {Result}"
    )]
    partial void LogPluginCallbackReturnedSynchronously(
        string infoTypeName,
        LuxonGamePluginResult? result
    );

    [LoggerMessage(LogLevel.Debug, "Plugin suspended without setting result")]
    partial void LogPluginSuspendedWithoutSettingResult();

    [LoggerMessage(
        LogLevel.Debug,
        "Suspended command and set up callbacks - returning LuxonGamePluginResult.Async"
    )]
    partial void LogSuspendedCommand();

    [LoggerMessage(LogLevel.Trace, "Event raised: 0x{evt:}X")]
    partial void LogEventRaised(byte evt);

    [LoggerMessage(
        LogLevel.Debug,
        "Plugin callback set result ({TypeName}) after async suspension: {Result}"
    )]
    partial void LogPluginCallbackSetResultAsynchronously(
        string typeName,
        LuxonGamePluginResult result
    );
}
