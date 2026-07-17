using System.Diagnostics;
using System.Runtime.InteropServices;
using Luxon.Server.Interop;

namespace Luxon.Server;

public delegate void GamePluginOnAttachDelegate(uint pluginId, Game game);
public delegate LuxonGamePluginResult GamePluginOnCreateGameDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    Peer creator,
    bool isJoin,
    bool createIfNotExist
);
public delegate LuxonGamePluginResult GamePluginBeforeJoinDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    Peer joiner
);
public delegate LuxonGamePluginResult GamePluginOnJoinGameDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    GamePeer joiner
);
public delegate LuxonGamePluginResult GamePluginOnLeaveDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    GamePeer leaver
);
public delegate LuxonGamePluginResult GamePluginOnRaiseEventDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    GamePeer raiser,
    LuxonEvent evt,
    byte cacheOp
);
public delegate LuxonGamePluginResult GamePluginBeforeSetPropertiesDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    GamePeer setter,
    bool broadcast,
    int targetActorId,
    SerValueRef update,
    SerValueRef expected
);
public delegate LuxonGamePluginResult GamePluginOnSetPropertiesDelegate(
    uint pluginId,
    Game game,
    SerMessageRef request,
    GamePeer setter,
    bool broadcast,
    int targetActorId,
    SerValueRef update,
    SerValueRef expected
);
public delegate LuxonGamePluginResult GamePluginBeforeCloseGameDelegate(
    uint pluginId,
    Game game,
    bool failedOnCreate
);
public delegate LuxonGamePluginResult GamePluginOnCloseGameDelegate(
    uint pluginId,
    Game game,
    bool failedOnCreate
);
public delegate bool AuthPluginOnAuthenticateDelegate(
    uint type,
    string? requestedUserId,
    string? parameters,
    string? data,
    string? secret,
    string? authUrl,
    out string? userId,
    SerMessageRef errorResponse
);
public delegate void CustomLogSinkDelegate(LuxonLogLevel level, string message);
public delegate bool HookpointMasterServerHandleOperationRequestJoinGameDelegate(
    HandlerBase handler,
    string gameId,
    bool isJoin
);
public delegate bool HookpointMasterServerHandleOperationRequestCreateGameDelegate(
    HandlerBase handler,
    string gameId
);
public delegate bool HookpointHandlerBaseHandleENetCommandOnMessageDelegate(
    HandlerBase handler,
    SerMessageRef message
);
public delegate bool HookpointAppLoadAppSettingsDelegate(IntPtr app, out bool success);

public sealed class LuxonServerImports
{
    public GamePluginOnAttachDelegate? GamePluginOnAttach { get; init; }
    public GamePluginOnCreateGameDelegate? GamePluginOnCreateGame { get; init; }
    public GamePluginBeforeJoinDelegate? GamePluginBeforeJoin { get; init; }
    public GamePluginOnJoinGameDelegate? GamePluginOnJoinGame { get; init; }
    public GamePluginOnLeaveDelegate? GamePluginOnLeave { get; init; }
    public GamePluginOnRaiseEventDelegate? GamePluginOnRaiseEvent { get; init; }
    public GamePluginBeforeSetPropertiesDelegate? GamePluginBeforeSetProperties { get; init; }
    public GamePluginOnSetPropertiesDelegate? GamePluginOnSetProperties { get; init; }
    public GamePluginBeforeCloseGameDelegate? GamePluginBeforeCloseGame { get; init; }
    public GamePluginOnCloseGameDelegate? GamePluginOnCloseGame { get; init; }
    public AuthPluginOnAuthenticateDelegate? AuthPluginOnAuthenticate { get; init; }
    public CustomLogSinkDelegate? CustomLogSink { get; init; }
    public HookpointMasterServerHandleOperationRequestJoinGameDelegate? HookpointMasterServerHandleOperationRequestJoinGame { get; init; }
    public HookpointMasterServerHandleOperationRequestCreateGameDelegate? HookpointMasterServerHandleOperationRequestCreateGame { get; init; }
    public HookpointHandlerBaseHandleENetCommandOnMessageDelegate? HookpointHandlerBaseHandleENetCommandOnMessage { get; init; }
    public HookpointAppLoadAppSettingsDelegate? HookpointAppLoadAppSettings { get; init; }
}

public sealed class LuxonServerImportsRegistration : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeGamePluginOnAttachDelegate(uint pluginId, IntPtr game);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginOnCreateGameDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr creator,
        byte isJoin,
        byte createIfNotExist
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginBeforeJoinDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr joiner
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginOnJoinGameDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr joiner
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginOnLeaveDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr leaver
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginOnRaiseEventDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr raiser,
        IntPtr evt,
        byte cacheOp
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginBeforeSetPropertiesDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr setter,
        byte broadcast,
        int targetActorId,
        IntPtr update,
        IntPtr expected
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginOnSetPropertiesDelegate(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr setter,
        byte broadcast,
        int targetActorId,
        IntPtr update,
        IntPtr expected
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginBeforeCloseGameDelegate(
        uint pluginId,
        IntPtr game,
        byte failedOnCreate
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeGamePluginOnCloseGameDelegate(
        uint pluginId,
        IntPtr game,
        byte failedOnCreate
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeAuthPluginOnAuthenticateDelegate(
        uint type,
        IntPtr requestedUserId,
        IntPtr parameters,
        IntPtr data,
        IntPtr secret,
        IntPtr authUrl,
        IntPtr outUserId,
        nuint maxUserIdLen,
        IntPtr outErrResp
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NativeCustomLogSinkDelegate(int level, IntPtr message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeHookpointJoinGameDelegate(
        IntPtr handler,
        IntPtr gameId,
        byte isJoin
    );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeHookpointCreateGameDelegate(IntPtr handler, IntPtr gameId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeHookpointOnMessageDelegate(IntPtr handler, IntPtr message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate byte NativeHookpointAppLoadAppSettingsDelegate(IntPtr app, IntPtr outSuccess);

    private readonly LuxonServerImports _imports;
    private readonly NativeImportsStruct _nativeImports;
    private int _disposed;

    private readonly NativeGamePluginOnAttachDelegate? _gamePluginOnAttachThunk;
    private readonly NativeGamePluginOnCreateGameDelegate? _gamePluginOnCreateGameThunk;
    private readonly NativeGamePluginBeforeJoinDelegate? _gamePluginBeforeJoinThunk;
    private readonly NativeGamePluginOnJoinGameDelegate? _gamePluginOnJoinGameThunk;
    private readonly NativeGamePluginOnLeaveDelegate? _gamePluginOnLeaveThunk;
    private readonly NativeGamePluginOnRaiseEventDelegate? _gamePluginOnRaiseEventThunk;
    private readonly NativeGamePluginBeforeSetPropertiesDelegate? _gamePluginBeforeSetPropertiesThunk;
    private readonly NativeGamePluginOnSetPropertiesDelegate? _gamePluginOnSetPropertiesThunk;
    private readonly NativeGamePluginBeforeCloseGameDelegate? _gamePluginBeforeCloseGameThunk;
    private readonly NativeGamePluginOnCloseGameDelegate? _gamePluginOnCloseGameThunk;
    private readonly NativeAuthPluginOnAuthenticateDelegate? _authPluginOnAuthenticateThunk;
    private readonly NativeCustomLogSinkDelegate? _customLogSinkThunk;
    private readonly NativeHookpointJoinGameDelegate? _hookpointJoinGameThunk;
    private readonly NativeHookpointCreateGameDelegate? _hookpointCreateGameThunk;
    private readonly NativeHookpointOnMessageDelegate? _hookpointOnMessageThunk;
    private readonly NativeHookpointAppLoadAppSettingsDelegate? _hookpointAppLoadAppSettingsThunk;

    internal LuxonServerImportsRegistration(LuxonServerImports imports)
    {
        _imports = imports ?? throw new ArgumentNullException(nameof(imports));

        _gamePluginOnAttachThunk = imports.GamePluginOnAttach is null ? null : OnGamePluginOnAttach;
        _gamePluginOnCreateGameThunk = imports.GamePluginOnCreateGame is null
            ? null
            : OnGamePluginOnCreateGame;
        _gamePluginBeforeJoinThunk = imports.GamePluginBeforeJoin is null
            ? null
            : OnGamePluginBeforeJoin;
        _gamePluginOnJoinGameThunk = imports.GamePluginOnJoinGame is null
            ? null
            : OnGamePluginOnJoinGame;
        _gamePluginOnLeaveThunk = imports.GamePluginOnLeave is null ? null : OnGamePluginOnLeave;
        _gamePluginOnRaiseEventThunk = imports.GamePluginOnRaiseEvent is null
            ? null
            : OnGamePluginOnRaiseEvent;
        _gamePluginBeforeSetPropertiesThunk = imports.GamePluginBeforeSetProperties is null
            ? null
            : OnGamePluginBeforeSetProperties;
        _gamePluginOnSetPropertiesThunk = imports.GamePluginOnSetProperties is null
            ? null
            : OnGamePluginOnSetProperties;
        _gamePluginBeforeCloseGameThunk = imports.GamePluginBeforeCloseGame is null
            ? null
            : OnGamePluginBeforeCloseGame;
        _gamePluginOnCloseGameThunk = imports.GamePluginOnCloseGame is null
            ? null
            : OnGamePluginOnCloseGame;
        _authPluginOnAuthenticateThunk = imports.AuthPluginOnAuthenticate is null
            ? null
            : OnAuthPluginOnAuthenticate;
        _customLogSinkThunk = imports.CustomLogSink is null ? null : OnCustomLogSink;
        _hookpointJoinGameThunk = imports.HookpointMasterServerHandleOperationRequestJoinGame
            is null
            ? null
            : OnHookpointJoinGame;
        _hookpointCreateGameThunk = imports.HookpointMasterServerHandleOperationRequestCreateGame
            is null
            ? null
            : OnHookpointCreateGame;
        _hookpointOnMessageThunk = imports.HookpointHandlerBaseHandleENetCommandOnMessage is null
            ? null
            : OnHookpointOnMessage;
        _hookpointAppLoadAppSettingsThunk = imports.HookpointAppLoadAppSettings is null
            ? null
            : OnHookpointAppLoadAppSettings;

        _nativeImports = new NativeImportsStruct
        {
            gamePluginOnAttach = ToPointer(_gamePluginOnAttachThunk),
            gamePluginOnCreateGame = ToPointer(_gamePluginOnCreateGameThunk),
            gamePluginBeforeJoin = ToPointer(_gamePluginBeforeJoinThunk),
            gamePluginOnJoinGame = ToPointer(_gamePluginOnJoinGameThunk),
            gamePluginOnLeave = ToPointer(_gamePluginOnLeaveThunk),
            gamePluginOnRaiseEvent = ToPointer(_gamePluginOnRaiseEventThunk),
            gamePluginBeforeSetProperties = ToPointer(_gamePluginBeforeSetPropertiesThunk),
            gamePluginOnSetProperties = ToPointer(_gamePluginOnSetPropertiesThunk),
            gamePluginBeforeCloseGame = ToPointer(_gamePluginBeforeCloseGameThunk),
            gamePluginOnCloseGame = ToPointer(_gamePluginOnCloseGameThunk),
            authPluginOnAuthenticate = ToPointer(_authPluginOnAuthenticateThunk),
            customLogSink = ToPointer(_customLogSinkThunk),
            hookpointMasterServerHandleOperationRequestJoinGame = ToPointer(
                _hookpointJoinGameThunk
            ),
            hookpointMasterServerHandleOperationRequestCreateGame = ToPointer(
                _hookpointCreateGameThunk
            ),
            hookpointHandlerBaseHandleENetCommandOnMessage = ToPointer(_hookpointOnMessageThunk),
            hookpointAppLoadAppSettings = ToPointer(_hookpointAppLoadAppSettingsThunk),
        };
    }

    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    internal void Apply()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LuxonServerImportsRegistration));
        }

        var imports = _nativeImports;
        LuxonInteropHelpers.Checked(() => NativeMethods.luxonSetServerImports(ref imports));
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        LuxonServerRuntime.ReleaseRegistration(this);
    }

    private static IntPtr ToPointer(Delegate? value) =>
        value is null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(value);

    private void OnGamePluginOnAttach(uint pluginId, IntPtr game)
    {
        try
        {
            _imports.GamePluginOnAttach?.Invoke(pluginId, new Game(game));
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
        }
    }

    private byte OnGamePluginOnCreateGame(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr creator,
        byte isJoin,
        byte createIfNotExist
    )
    {
        try
        {
            var callback = _imports.GamePluginOnCreateGame;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            var result = callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new Peer(creator),
                LuxonInteropHelpers.ByteToBool(isJoin),
                LuxonInteropHelpers.ByteToBool(createIfNotExist)
            );

            return (byte)result;
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginBeforeJoin(uint pluginId, IntPtr game, IntPtr request, IntPtr joiner)
    {
        try
        {
            var callback = _imports.GamePluginBeforeJoin;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new Peer(joiner)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginOnJoinGame(uint pluginId, IntPtr game, IntPtr request, IntPtr joiner)
    {
        try
        {
            var callback = _imports.GamePluginOnJoinGame;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new GamePeer(joiner, false)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginOnLeave(uint pluginId, IntPtr game, IntPtr request, IntPtr leaver)
    {
        try
        {
            var callback = _imports.GamePluginOnLeave;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new GamePeer(leaver, false)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginOnRaiseEvent(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr raiser,
        IntPtr evt,
        byte cacheOp
    )
    {
        try
        {
            var callback = _imports.GamePluginOnRaiseEvent;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new GamePeer(raiser, false),
                new LuxonEvent(evt, false),
                cacheOp
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginBeforeSetProperties(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr setter,
        byte broadcast,
        int targetActorId,
        IntPtr update,
        IntPtr expected
    )
    {
        try
        {
            var callback = _imports.GamePluginBeforeSetProperties;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new GamePeer(setter, false),
                LuxonInteropHelpers.ByteToBool(broadcast),
                targetActorId,
                new SerValueRef(update, false),
                new SerValueRef(expected, false)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginOnSetProperties(
        uint pluginId,
        IntPtr game,
        IntPtr request,
        IntPtr setter,
        byte broadcast,
        int targetActorId,
        IntPtr update,
        IntPtr expected
    )
    {
        try
        {
            var callback = _imports.GamePluginOnSetProperties;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                new SerMessageRef(request, false),
                new GamePeer(setter, false),
                LuxonInteropHelpers.ByteToBool(broadcast),
                targetActorId,
                new SerValueRef(update, false),
                new SerValueRef(expected, false)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginBeforeCloseGame(uint pluginId, IntPtr game, byte failedOnCreate)
    {
        try
        {
            var callback = _imports.GamePluginBeforeCloseGame;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                LuxonInteropHelpers.ByteToBool(failedOnCreate)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnGamePluginOnCloseGame(uint pluginId, IntPtr game, byte failedOnCreate)
    {
        try
        {
            var callback = _imports.GamePluginOnCloseGame;
            if (callback is null)
            {
                return (byte)LuxonGamePluginResult.Continue;
            }

            return (byte)callback(
                pluginId,
                new Game(game),
                LuxonInteropHelpers.ByteToBool(failedOnCreate)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return (byte)LuxonGamePluginResult.Fail;
        }
    }

    private byte OnAuthPluginOnAuthenticate(
        uint type,
        IntPtr requestedUserId,
        IntPtr parameters,
        IntPtr data,
        IntPtr secret,
        IntPtr authUrl,
        IntPtr outUserId,
        nuint maxUserIdLen,
        IntPtr outErrResp
    )
    {
        try
        {
            var callback = _imports.AuthPluginOnAuthenticate;
            if (callback is null)
            {
                LuxonInteropHelpers.WriteUtf8CString(null, outUserId, maxUserIdLen);
                return 0;
            }

            var ok = callback(
                type,
                LuxonInteropHelpers.PtrToUtf8String(requestedUserId),
                LuxonInteropHelpers.PtrToUtf8String(parameters),
                LuxonInteropHelpers.PtrToUtf8String(data),
                LuxonInteropHelpers.PtrToUtf8String(secret),
                LuxonInteropHelpers.PtrToUtf8String(authUrl),
                out var userId,
                new SerMessageRef(outErrResp, false)
            );

            LuxonInteropHelpers.WriteUtf8CString(userId, outUserId, maxUserIdLen);
            return LuxonInteropHelpers.BoolToByte(ok);
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            LuxonInteropHelpers.WriteUtf8CString(null, outUserId, maxUserIdLen);
            return 0;
        }
    }

    private void OnCustomLogSink(int level, IntPtr message)
    {
        try
        {
            _imports.CustomLogSink?.Invoke(
                (LuxonLogLevel)level,
                LuxonInteropHelpers.PtrToUtf8String(message)
            );
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
        }
    }

    private byte OnHookpointJoinGame(IntPtr handler, IntPtr gameId, byte isJoin)
    {
        try
        {
            var callback = _imports.HookpointMasterServerHandleOperationRequestJoinGame;
            if (callback is null)
            {
                return 0;
            }

            var handled = callback(
                new HandlerBase(handler),
                LuxonInteropHelpers.PtrToUtf8String(gameId),
                LuxonInteropHelpers.ByteToBool(isJoin)
            );

            return LuxonInteropHelpers.BoolToByte(handled);
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return 0;
        }
    }

    private byte OnHookpointCreateGame(IntPtr handler, IntPtr gameId)
    {
        try
        {
            var callback = _imports.HookpointMasterServerHandleOperationRequestCreateGame;
            if (callback is null)
            {
                return 0;
            }

            var handled = callback(
                new HandlerBase(handler),
                LuxonInteropHelpers.PtrToUtf8String(gameId)
            );
            return LuxonInteropHelpers.BoolToByte(handled);
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return 0;
        }
    }

    private byte OnHookpointOnMessage(IntPtr handler, IntPtr message)
    {
        try
        {
            var callback = _imports.HookpointHandlerBaseHandleENetCommandOnMessage;
            if (callback is null)
            {
                return 0;
            }

            var handled = callback(new HandlerBase(handler), new SerMessageRef(message, false));
            return LuxonInteropHelpers.BoolToByte(handled);
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            return 0;
        }
    }

    private byte OnHookpointAppLoadAppSettings(IntPtr app, IntPtr outSuccess)
    {
        try
        {
            var callback = _imports.HookpointAppLoadAppSettings;
            if (callback is null)
            {
                LuxonInteropHelpers.WriteBoolByte(outSuccess, false);
                return 0;
            }

            var handled = callback(app, out var success);
            LuxonInteropHelpers.WriteBoolByte(outSuccess, success);
            return LuxonInteropHelpers.BoolToByte(handled);
        }
        catch (Exception ex)
        {
            LuxonServerRuntime.ReportCallbackException(ex);
            LuxonInteropHelpers.WriteBoolByte(outSuccess, false);
            return 0;
        }
    }
}

public static class LuxonServerRuntime
{
    private static readonly object s_sync = new();
    private static LuxonServerImportsRegistration? s_currentImports;

    public static event Action<Exception>? UnhandledCallbackException;

    public static string NativeLibraryName => NativeMethods.LibraryName;

    public static void UseCustomNativeLibraryPath(string path) =>
        LuxonNativeLibrary.SetCustomLibraryPath(path);

    public static void ClearCustomNativeLibraryPath() =>
        LuxonNativeLibrary.ClearCustomLibraryPath();

    public static LuxonServerImportsRegistration SetImports(LuxonServerImports imports)
    {
        ArgumentNullException.ThrowIfNull(imports);

        var registration = new LuxonServerImportsRegistration(imports);

        lock (s_sync)
        {
            registration.Apply();
            s_currentImports = registration;
        }

        return registration;
    }

    public static void ClearImports()
    {
        lock (s_sync)
        {
            var empty = default(NativeImportsStruct);
            LuxonInteropHelpers.Checked(() => NativeMethods.luxonSetServerImports(ref empty));
            s_currentImports = null;
        }
    }

    public static void EnableCustomLogSink(bool enabled)
    {
        try
        {
            LuxonInteropHelpers.Checked(() => NativeMethods.luxonEnableCustomLogSink(enabled));
        }
        catch (EntryPointNotFoundException ex)
        {
            throw new LuxonFeatureUnavailableException("custom log sink", ex);
        }
    }

    public static uint RegisterGamePlugin(string pluginName)
    {
        ArgumentException.ThrowIfNullOrEmpty(pluginName);

        try
        {
            return LuxonInteropHelpers.Checked(() => NativeMethods.registerGamePlugin(pluginName));
        }
        catch (EntryPointNotFoundException ex)
        {
            throw new LuxonFeatureUnavailableException("plugins", ex);
        }
    }

    public static bool RegisterAuthPlugin(uint type)
    {
        try
        {
            return LuxonInteropHelpers.Checked(() => NativeMethods.registerAuthPlugin(type));
        }
        catch (EntryPointNotFoundException ex)
        {
            throw new LuxonFeatureUnavailableException("auth plugins", ex);
        }
    }

    public static void RegisterHookpoints(ServerManager manager)
    {
        LuxonInteropHelpers.ThrowIfDisposed(manager, nameof(manager));

        try
        {
            LuxonInteropHelpers.Checked(() =>
                NativeMethods.registerHookpoints(manager.DangerousHandle)
            );
        }
        catch (EntryPointNotFoundException ex)
        {
            throw new LuxonFeatureUnavailableException("hookpoints", ex);
        }
    }

    internal static void ReleaseRegistration(LuxonServerImportsRegistration registration)
    {
        lock (s_sync)
        {
            if (!ReferenceEquals(s_currentImports, registration))
            {
                return;
            }

            var empty = default(NativeImportsStruct);
            LuxonInteropHelpers.Checked(() => NativeMethods.luxonSetServerImports(ref empty));
            s_currentImports = null;
        }
    }

    internal static void ReportCallbackException(Exception ex)
    {
        try
        {
            UnhandledCallbackException?.Invoke(ex);
        }
        catch { }

        Trace.TraceError(ex.ToString());
    }
}
