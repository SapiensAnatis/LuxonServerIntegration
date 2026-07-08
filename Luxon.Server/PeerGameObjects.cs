using Luxon.Server.Interop;

namespace Luxon.Server;

public sealed class Logger
{
    private readonly IntPtr _handle;

    internal Logger(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Native handle cannot be zero.", nameof(handle));
        }

        _handle = handle;
    }

    public IntPtr DangerousHandle => _handle;

    public static Logger GetOrCreate(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return new Logger(LuxonInteropHelpers.Checked(() => NativeMethods.getOrCreateLogger(name)));
    }

    public void SetLevel(LuxonLogLevel level)
        => LuxonInteropHelpers.Checked(() => NativeMethods.setLoggerLevel(_handle, (int)level));

    public void Trace(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        LuxonInteropHelpers.Checked(() => NativeMethods.loggerTrace(_handle, message));
    }

    public void Debug(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        LuxonInteropHelpers.Checked(() => NativeMethods.loggerDebug(_handle, message));
    }

    public void Info(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        LuxonInteropHelpers.Checked(() => NativeMethods.loggerInfo(_handle, message));
    }

    public void Warn(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        LuxonInteropHelpers.Checked(() => NativeMethods.loggerWarn(_handle, message));
    }

    public void Error(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        LuxonInteropHelpers.Checked(() => NativeMethods.loggerError(_handle, message));
    }

    public void Critical(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        LuxonInteropHelpers.Checked(() => NativeMethods.loggerCritical(_handle, message));
    }

    public void Log(LuxonLogLevel level, string message)
    {
        switch (level)
        {
            case LuxonLogLevel.Trace:
                Trace(message);
                break;
            case LuxonLogLevel.Debug:
                Debug(message);
                break;
            case LuxonLogLevel.Info:
                Info(message);
                break;
            case LuxonLogLevel.Warn:
                Warn(message);
                break;
            case LuxonLogLevel.Error:
                Error(message);
                break;
            case LuxonLogLevel.Critical:
                Critical(message);
                break;
            case LuxonLogLevel.Off:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }
}

public sealed class Peer
{
    private readonly IntPtr _handle;

    internal Peer(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Native handle cannot be zero.", nameof(handle));
        }

        _handle = handle;
    }

    internal IntPtr Handle => _handle;

    public IntPtr DangerousHandle => _handle;

    public bool IsAuthenticated => LuxonInteropHelpers.Checked(() => NativeMethods.peerIsAuthenticated(_handle));

    public LuxonServerProtocol TransportProtocol => LuxonInteropHelpers.Checked(() => NativeMethods.peerGetTransportProtocol(_handle));

    public string UserId => LuxonInteropHelpers.PtrToUtf8String(LuxonInteropHelpers.Checked(() => NativeMethods.peerGetUserId(_handle)));

    public Game? CurrentGame
    {
        get
        {
            var handle = LuxonInteropHelpers.Checked(() => NativeMethods.peerGetCurrentGame(_handle));
            return handle == IntPtr.Zero ? null : new Game(handle);
        }
    }

    public void Send(ReadOnlySpan<byte> payload, byte channel = 0, LuxonDeliveryMode deliveryMode = LuxonDeliveryMode.Reliable)
    {
        var data = payload.ToArray();
        LuxonInteropHelpers.Checked(() => NativeMethods.peerSend(_handle, data, (nuint)data.Length, channel, deliveryMode));
    }

    public void Disconnect()
        => LuxonInteropHelpers.Checked(() => NativeMethods.peerDisconnect(_handle));
}

public sealed class GamePeer : NativeResource
{
    internal GamePeer(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public global::Luxon.Ser.Value ActorProperties
    {
        get => GetActorProperties();
        set => SetActorProperties(value);
    }

    public static GamePeer CreateContainer()
        => new(LuxonInteropHelpers.Checked(NativeMethods.createGamePeerContainer), true);

    internal void TransferOwnershipToNative()
        => _ = DetachHandle();

    public bool IsValid => LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerIsValid(Handle));

    public int ActorId => LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerGetActorId(Handle));

    public bool HasInterestGroup(byte group)
        => LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerHasInterestGroup(Handle, group));

    public void SetInterestGroup(byte group, bool enable)
        => LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerSetInterestGroup(Handle, group, enable));

    public uint[] GetInterestGroupsMask(int wordCount = 8)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(wordCount);

        if (wordCount == 0)
        {
            return Array.Empty<uint>();
        }

        var words = new uint[wordCount];
        LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerGetInterestGroupsMask(Handle, words, (nuint)words.Length));
        return words;
    }

    public void SetInterestGroupsMask(ReadOnlySpan<uint> maskWords)
    {
        var words = maskWords.ToArray();
        LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerSetInterestGroupsMask(Handle, words, (nuint)words.Length));
    }

    public SerValueRef GetActorPropertiesNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerGetActorProps(Handle, outHandle)));

    public global::Luxon.Ser.Value GetActorProperties()
    {
        using var native = GetActorPropertiesNative();
        return native.ToManagedValue();
    }

    public void SetActorProperties(SerValueRef props)
    {
        LuxonInteropHelpers.ThrowIfDisposed(props, nameof(props));
        LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerSetActorProps(Handle, props.DangerousHandle));
    }

    public void SetActorProperties(global::Luxon.Ser.Value props)
    {
        using var native = SerValueRef.FromManaged(props);
        SetActorProperties(native);
    }

    public Peer? BasePeer
    {
        get
        {
            var handle = LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerGetBasePeer(Handle));
            return handle == IntPtr.Zero ? null : new Peer(handle);
        }
    }

    public bool Disconnect()
        => LuxonInteropHelpers.Checked(() => NativeMethods.gamePeerDisconnect(Handle));

    protected override void ReleaseHandle(IntPtr handle)
        => NativeMethods.destroyGamePeerContainer(handle);
}

public sealed class LuxonEvent : NativeResource
{
    internal LuxonEvent(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public EventRoutingMetadata RoutingMetadata
    {
        get => GetRoutingMetadata();
        set => SetRoutingMetadata(value);
    }

    public global::Luxon.Ser.Value Data
    {
        get => GetData();
        set => SetData(value);
    }

    public global::Luxon.Ser.Value TopParams
    {
        get => GetTopParams();
        set => SetTopParams(value);
    }

    public static LuxonEvent Create()
        => new(LuxonInteropHelpers.Checked(NativeMethods.createEvent), true);

    public void SetRoutingMetadata(EventRoutingMetadata metadata)
        => SetRoutingMetadata(metadata.Code, metadata.SenderActorId, metadata.DeliveryMode, metadata.Channel, metadata.InterestGroup);

    public void SetRoutingMetadata(byte code, int senderActorId, LuxonDeliveryMode deliveryMode, byte channel, byte interestGroup)
        => LuxonInteropHelpers.Checked(() => NativeMethods.eventSetRoutingMetadata(Handle, code, senderActorId, deliveryMode, channel, interestGroup));

    public EventRoutingMetadata GetRoutingMetadata()
    {
        byte code = 0;
        int senderActorId = 0;
        LuxonDeliveryMode deliveryMode = default;
        byte channel = 0;
        byte interestGroup = 0;

        LuxonInteropHelpers.Checked(() => NativeMethods.eventGetRoutingMetadata(Handle, out code, out senderActorId, out deliveryMode, out channel, out interestGroup));
        return new EventRoutingMetadata(code, senderActorId, deliveryMode, channel, interestGroup);
    }

    public void SetReceiversAll()
        => LuxonInteropHelpers.Checked(() => NativeMethods.eventSetReceiversAll(Handle));

    public void SetReceiversGroup(byte group)
        => LuxonInteropHelpers.Checked(() => NativeMethods.eventSetReceiversGroup(Handle, group));

    public void SetReceiversActors(ReadOnlySpan<int> actorIds)
    {
        var ids = actorIds.ToArray();
        LuxonInteropHelpers.Checked(() => NativeMethods.eventSetReceiversActors(Handle, ids, (nuint)ids.Length));
    }

    public LuxonEventReceiversType GetReceiversType()
        => LuxonInteropHelpers.Checked(() => NativeMethods.eventGetReceiversType(Handle));

    public byte GetReceiversGroup()
        => LuxonInteropHelpers.Checked(() => NativeMethods.eventGetReceiversGroup(Handle));

    public int[] GetReceiversActors(int maxCount = 256)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount);

        if (maxCount == 0)
        {
            return Array.Empty<int>();
        }

        var actorIds = new int[maxCount];
        nuint written = 0;

        LuxonInteropHelpers.Checked(() => NativeMethods.eventGetReceiversActors(Handle, actorIds, (nuint)actorIds.Length, out written));

        var actual = written > (nuint)actorIds.Length ? actorIds.Length : (int)written;
        if (actual != actorIds.Length)
        {
            Array.Resize(ref actorIds, actual);
        }

        return actorIds;
    }

    public SerValueRef GetDataNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.eventGetData(Handle, outHandle)));

    public global::Luxon.Ser.Value GetData()
    {
        using var native = GetDataNative();
        return native.ToManagedValue();
    }

    public void SetData(SerValueRef data)
    {
        LuxonInteropHelpers.ThrowIfDisposed(data, nameof(data));
        LuxonInteropHelpers.Checked(() => NativeMethods.eventSetData(Handle, data.DangerousHandle));
    }

    public void SetData(global::Luxon.Ser.Value data)
    {
        using var native = SerValueRef.FromManaged(data);
        SetData(native);
    }

    public SerValueRef GetTopParamsNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.eventGetTopParams(Handle, outHandle)));

    public global::Luxon.Ser.Value GetTopParams()
    {
        using var native = GetTopParamsNative();
        return native.ToManagedValue();
    }

    public void SetTopParams(SerValueRef topParams)
    {
        LuxonInteropHelpers.ThrowIfDisposed(topParams, nameof(topParams));
        LuxonInteropHelpers.Checked(() => NativeMethods.eventSetTopParams(Handle, topParams.DangerousHandle));
    }

    public void SetTopParams(global::Luxon.Ser.Value topParams)
    {
        using var native = SerValueRef.FromManaged(topParams);
        SetTopParams(native);
    }

    protected override void ReleaseHandle(IntPtr handle)
        => NativeMethods.destroyEvent(handle);
}

public sealed class Game
{
    private readonly IntPtr _handle;
    private string? _cachedId;

    internal Game(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Native handle cannot be zero.", nameof(handle));
        }

        _handle = handle;
    }

    internal IntPtr Handle => _handle;

    public IntPtr DangerousHandle => _handle;

    public string Id => _cachedId ??= GetId();

    public GameConfigState ConfigState
    {
        get => GetConfigState();
        set => SetConfigState(value);
    }

    public byte MaxPeers
    {
        get => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetMaxPeers(_handle));
        set => LuxonInteropHelpers.Checked(() => NativeMethods.gameSetMaxPeers(_handle, value));
    }

    public int MasterActor
    {
        get => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetMasterActor(_handle));
        set => LuxonInteropHelpers.Checked(() => NativeMethods.gameSetMasterActor(_handle, value));
    }

    public int LastActorId => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetLastActorId(_handle));

    public GameTtlConfig TtlConfig
    {
        get => GetTtlConfig();
        set => SetTtlConfig(value);
    }

    public global::Luxon.Ser.Value CustomProperties
    {
        get => GetCustomProperties();
        set => SetCustomProperties(value);
    }

    public global::Luxon.Ser.Value LobbyPropertiesKeys
    {
        get => GetLobbyPropertiesKeys();
        set => SetLobbyPropertiesKeys(value);
    }

    public global::Luxon.Ser.Value ExpectedUsers
    {
        get => GetExpectedUsers();
        set => SetExpectedUsers(value);
    }

    public nuint PeerCount => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetPeerCount(_handle));

    public ServerManager ServerManager => new(LuxonInteropHelpers.Checked(() => NativeMethods.gameGetServerManager(_handle)), false);

    public string GetId(int bufferSize = 4096)
    {
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        var buffer = new byte[bufferSize];
        LuxonInteropHelpers.Checked(() => NativeMethods.gameGetId(_handle, buffer, (nuint)buffer.Length));
        return LuxonInteropHelpers.DecodeUtf8(buffer);
    }

    public GameConfigState GetConfigState()
    {
        byte flags = 3; // CheckUserOnJoin | DeleteCacheOnLeave
        bool isCreated = false;
        bool isOpen = true;
        bool isVisible = true;
        byte maxPeers = 0;
        int masterActor = 1;

        LuxonInteropHelpers.Checked(() => NativeMethods.gameGetConfigState(_handle, out flags, out isCreated, out isOpen, out isVisible, out maxPeers, out masterActor));
        return new GameConfigState(flags, isCreated, isOpen, isVisible, maxPeers, masterActor);
    }

    public void SetConfigState(GameConfigState state)
        => SetConfigState(state.Flags, state.IsOpen, state.IsVisible, state.MaxPeers, state.MasterActor);

    public void SetConfigState(byte flags, bool isOpen, bool isVisible, byte maxPeers, int masterActor)
        => LuxonInteropHelpers.Checked(() => NativeMethods.gameSetConfigState(_handle, flags, isOpen, isVisible, maxPeers, masterActor));

    public GameTtlConfig GetTtlConfig()
    {
        int playerTtl = 0;
        int emptyTtl = 0;

        LuxonInteropHelpers.Checked(() => NativeMethods.gameGetTtlConfig(_handle, out playerTtl, out emptyTtl));
        return new GameTtlConfig(playerTtl, emptyTtl);
    }

    public void SetTtlConfig(GameTtlConfig config)
        => SetTtlConfig(config.PlayerTtl, config.EmptyTtl);

    public void SetTtlConfig(int playerTtl, int emptyTtl)
        => LuxonInteropHelpers.Checked(() => NativeMethods.gameSetTtlConfig(_handle, playerTtl, emptyTtl));

    public SerValueRef GetCustomPropertiesNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetCustomProps(_handle, outHandle)));

    public global::Luxon.Ser.Value GetCustomProperties()
    {
        using var native = GetCustomPropertiesNative();
        return native.ToManagedValue();
    }

    public void SetCustomProperties(SerValueRef customProps)
    {
        LuxonInteropHelpers.ThrowIfDisposed(customProps, nameof(customProps));
        LuxonInteropHelpers.Checked(() => NativeMethods.gameSetCustomProps(_handle, customProps.DangerousHandle));
    }

    public void SetCustomProperties(global::Luxon.Ser.Value customProps)
    {
        using var native = SerValueRef.FromManaged(customProps);
        SetCustomProperties(native);
    }

    public SerValueRef GetLobbyPropertiesKeysNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetLobbyPropsToSerValue(_handle, outHandle)));

    public global::Luxon.Ser.Value GetLobbyPropertiesKeys()
    {
        using var native = GetLobbyPropertiesKeysNative();
        return native.ToManagedValue();
    }

    public void SetLobbyPropertiesKeys(SerValueRef keysArray)
    {
        LuxonInteropHelpers.ThrowIfDisposed(keysArray, nameof(keysArray));
        LuxonInteropHelpers.Checked(() => NativeMethods.gameSetLobbyPropsFromSerValue(_handle, keysArray.DangerousHandle));
    }

    public void SetLobbyPropertiesKeys(global::Luxon.Ser.Value keysArray)
    {
        using var native = SerValueRef.FromManaged(keysArray);
        SetLobbyPropertiesKeys(native);
    }

    public SerValueRef GetExpectedUsersNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetExpectedUsersToSerValue(_handle, outHandle)));

    public global::Luxon.Ser.Value GetExpectedUsers()
    {
        using var native = GetExpectedUsersNative();
        return native.ToManagedValue();
    }

    public void SetExpectedUsers(SerValueRef users)
    {
        LuxonInteropHelpers.ThrowIfDisposed(users, nameof(users));
        LuxonInteropHelpers.Checked(() => NativeMethods.gameSetExpectedUsersFromSerValue(_handle, users.DangerousHandle));
    }

    public void SetExpectedUsers(global::Luxon.Ser.Value users)
    {
        using var native = SerValueRef.FromManaged(users);
        SetExpectedUsers(native);
    }

    public void AddExpectedUser(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        LuxonInteropHelpers.Checked(() => NativeMethods.gameAddExpectedUser(_handle, userId));
    }

    public void RemoveExpectedUser(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        LuxonInteropHelpers.Checked(() => NativeMethods.gameRemoveExpectedUser(_handle, userId));
    }

    public GamePeer CreatePeer(Peer peer)
    {
        ArgumentNullException.ThrowIfNull(peer);
        return new GamePeer(LuxonInteropHelpers.Checked(() => NativeMethods.gameCreatePeer(_handle, peer.Handle)), true);
    }

    public GamePeer AddPeer(GamePeer gamePeer)
    {
        LuxonInteropHelpers.ThrowIfDisposed(gamePeer, nameof(gamePeer));

        var handle = LuxonInteropHelpers.Checked(() => NativeMethods.gameAddPeer(_handle, gamePeer.DangerousHandle));
        gamePeer.TransferOwnershipToNative();
        return new GamePeer(handle, false);
    }

    public bool RemovePeer(Peer peer)
    {
        ArgumentNullException.ThrowIfNull(peer);
        return LuxonInteropHelpers.Checked(() => NativeMethods.gameRemovePeer(_handle, peer.Handle));
    }

    public bool FloodPeer(GamePeer gamePeer)
    {
        LuxonInteropHelpers.ThrowIfDisposed(gamePeer, nameof(gamePeer));
        return LuxonInteropHelpers.Checked(() => NativeMethods.gameFloodPeer(_handle, gamePeer.DangerousHandle));
    }

    public GamePeer? FindPeerByActorId(int actorId)
    {
        var handle = LuxonInteropHelpers.Checked(() => NativeMethods.gameFindPeerByActorId(_handle, actorId));
        return handle == IntPtr.Zero ? null : new GamePeer(handle, false);
    }

    public GamePeer? FindPeerByBasePeer(Peer peer)
    {
        ArgumentNullException.ThrowIfNull(peer);
        var handle = LuxonInteropHelpers.Checked(() => NativeMethods.gameFindPeerByBasePeer(_handle, peer.Handle));
        return handle == IntPtr.Zero ? null : new GamePeer(handle, false);
    }

    public void BroadcastEvent(LuxonEvent evt)
    {
        LuxonInteropHelpers.ThrowIfDisposed(evt, nameof(evt));
        LuxonInteropHelpers.Checked(() => NativeMethods.gameBroadcastEvent(_handle, evt.DangerousHandle));
    }

    public JoinValidationResult ValidateJoin(string userId, int newExpectedUsersCount = 0, int errorBufferSize = 4096)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        if (newExpectedUsersCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(newExpectedUsersCount));
        }

        if (errorBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(errorBufferSize));
        }

        var errorBuffer = new byte[errorBufferSize];
        short errorCode = 0;

        var allowed = LuxonInteropHelpers.Checked(() => NativeMethods.gameValidateJoin(
            _handle,
            userId,
            (nuint)newExpectedUsersCount,
            out errorCode,
            errorBuffer,
            (nuint)errorBuffer.Length));

        return new JoinValidationResult(allowed, errorCode, LuxonInteropHelpers.DecodeUtf8(errorBuffer));
    }

    public void TriggerLobbyUpdate()
        => LuxonInteropHelpers.Checked(() => NativeMethods.gameTriggerLobbyUpdate(_handle));

    public SerValueRef GetGamePropertyNative(SerValueRef key)
    {
        LuxonInteropHelpers.ThrowIfDisposed(key, nameof(key));
        return SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetGameProp(_handle, key.DangerousHandle, outHandle)));
    }

    public global::Luxon.Ser.Value GetGameProperty(global::Luxon.Ser.Value key)
    {
        using var nativeKey = SerValueRef.FromManaged(key);
        using var nativeValue = GetGamePropertyNative(nativeKey);
        return nativeValue.ToManagedValue();
    }

    public SerValueRef GetLobbyGamePropertiesNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetLobbyGameProps(_handle, outHandle)));

    public global::Luxon.Ser.Value GetLobbyGameProperties()
    {
        using var native = GetLobbyGamePropertiesNative();
        return native.ToManagedValue();
    }

    public SerValueRef GetGamePropertiesNative(bool noCustom = false)
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetGameProps(_handle, noCustom, outHandle)));

    public global::Luxon.Ser.Value GetGameProperties(bool noCustom = false)
    {
        using var native = GetGamePropertiesNative(noCustom);
        return native.ToManagedValue();
    }

    public SerValueRef GetActorPropertiesNative()
        => SerValueRef.CreatePopulated(outHandle => LuxonInteropHelpers.Checked(() => NativeMethods.gameGetActorProps(_handle, outHandle)));

    public global::Luxon.Ser.Value GetActorProperties()
    {
        using var native = GetActorPropertiesNative();
        return native.ToManagedValue();
    }

    public void InsertGameProperties(SerValueRef update, bool broadcast)
    {
        LuxonInteropHelpers.ThrowIfDisposed(update, nameof(update));
        LuxonInteropHelpers.Checked(() => NativeMethods.gameInsertGameProps(_handle, update.DangerousHandle, broadcast));
    }

    public void InsertGameProperties(global::Luxon.Ser.Value update, bool broadcast)
    {
        using var native = SerValueRef.FromManaged(update);
        InsertGameProperties(native, broadcast);
    }

    public bool ExpectGameProperties(SerValueRef expected)
    {
        LuxonInteropHelpers.ThrowIfDisposed(expected, nameof(expected));
        return LuxonInteropHelpers.Checked(() => NativeMethods.gameExpectGameProps(_handle, expected.DangerousHandle));
    }

    public bool ExpectGameProperties(global::Luxon.Ser.Value expected)
    {
        using var native = SerValueRef.FromManaged(expected);
        return ExpectGameProperties(native);
    }

    public bool InsertActorProperties(int actorId, SerValueRef update, bool broadcast)
    {
        LuxonInteropHelpers.ThrowIfDisposed(update, nameof(update));
        return LuxonInteropHelpers.Checked(() => NativeMethods.gameInsertActorProps(_handle, actorId, update.DangerousHandle, broadcast));
    }

    public bool InsertActorProperties(int actorId, global::Luxon.Ser.Value update, bool broadcast)
    {
        using var native = SerValueRef.FromManaged(update);
        return InsertActorProperties(actorId, native, broadcast);
    }

    public bool ExpectActorProperties(int actorId, SerValueRef expected)
    {
        LuxonInteropHelpers.ThrowIfDisposed(expected, nameof(expected));
        return LuxonInteropHelpers.Checked(() => NativeMethods.gameExpectActorProps(_handle, actorId, expected.DangerousHandle));
    }

    public bool ExpectActorProperties(int actorId, global::Luxon.Ser.Value expected)
    {
        using var native = SerValueRef.FromManaged(expected);
        return ExpectActorProperties(actorId, native);
    }

    public static bool MatchesFilter(SerValueRef eventData, SerValueRef filter)
    {
        LuxonInteropHelpers.ThrowIfDisposed(eventData, nameof(eventData));
        LuxonInteropHelpers.ThrowIfDisposed(filter, nameof(filter));
        return LuxonInteropHelpers.Checked(() => NativeMethods.gameMatchesFilter(eventData.DangerousHandle, filter.DangerousHandle));
    }

    public static bool MatchesFilter(global::Luxon.Ser.Value eventData, global::Luxon.Ser.Value filter)
    {
        using var nativeEventData = SerValueRef.FromManaged(eventData);
        using var nativeFilter = SerValueRef.FromManaged(filter);
        return MatchesFilter(nativeEventData, nativeFilter);
    }
}

public sealed class Lobby : NativeResource
{
    internal Lobby(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public static Lobby Create(string name, byte type)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return new Lobby(LuxonInteropHelpers.Checked(() => NativeMethods.createLobby(name, type)), true);
    }

    public CreateGameResult CreateGame(string id, bool orGet = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var errorResponse = SerMessageRef.Create();
        try
        {
            var handle = LuxonInteropHelpers.Checked(() => NativeMethods.lobbyCreateGame(Handle, id, orGet, errorResponse.DangerousHandle));
            return new CreateGameResult(handle == IntPtr.Zero ? null : new Game(handle), errorResponse);
        }
        catch
        {
            errorResponse.Dispose();
            throw;
        }
    }

    public bool TryCreateGame(string id, out CreateGameResult result, bool orGet = false)
    {
        result = CreateGame(id, orGet);
        return result.Game is not null;
    }

    public nuint PeerCount => LuxonInteropHelpers.Checked(() => NativeMethods.lobbyGetPeerCount(Handle));

    public nuint MasterPeerCount => LuxonInteropHelpers.Checked(() => NativeMethods.lobbyGetMasterPeerCount(Handle));

    public LobbyQueryResult QueryLobbies(string sqlQueries, int bufferSize = 65536)
    {
        ArgumentNullException.ThrowIfNull(sqlQueries);

        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        var buffer = new byte[bufferSize];
        nuint written = 0;

        var success = LuxonInteropHelpers.Checked(() => NativeMethods.lobbyQueryLobbies(
            Handle,
            sqlQueries,
            buffer,
            (nuint)buffer.Length,
            out written));

        return new LobbyQueryResult(success, LuxonInteropHelpers.DecodeUtf8(buffer, written));
    }

    protected override void ReleaseHandle(IntPtr handle)
        => NativeMethods.destroyLobby(handle);
}

public sealed class HandlerBase
{
    private readonly IntPtr _handle;

    internal HandlerBase(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Native handle cannot be zero.", nameof(handle));
        }

        _handle = handle;
    }

    internal IntPtr Handle => _handle;

    public IntPtr DangerousHandle => _handle;

    public ServerManager ServerManager
        => new(LuxonInteropHelpers.Checked(() => NativeMethods.handlerBaseGetServerManager(_handle)), false);
}
