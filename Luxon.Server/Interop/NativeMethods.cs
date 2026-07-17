using System.Runtime.InteropServices;

namespace Luxon.Server.Interop;

internal static class NativeMethods
{
    internal const string LibraryName = "luxon_server";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool luxonHasError();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr luxonGetLastErrorType();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr luxonGetLastErrorMessage();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void luxonClearLastError();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createByteArray();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyByteArray(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern nuint copyFromByteArray(IntPtr val, byte[] out_buf, nuint max_len);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void appendToByteArray(IntPtr val, byte[] buf, nuint len);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void copyToByteArray(IntPtr val, byte[] buf, nuint len);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern nuint getByteArraySize(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr getByteArrayDataPtr(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void pushToByteArray(IntPtr val, byte value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern byte getInByteArray(IntPtr val, nuint index);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void setInByteArray(IntPtr val, nuint index, byte value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createSerMessage();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroySerMessage(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr serializeSerMessage(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool deserializeSerMessage(byte[] buf, nuint len, IntPtr out_val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool deserializeSerMessageFromByteArray(IntPtr val, IntPtr out_val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createSerValue();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroySerValue(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr serializeSerValue(IntPtr val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool deserializeSerValue(byte[] buf, nuint len, IntPtr out_val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool deserializeSerValueFromByteArray(IntPtr val, IntPtr out_val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void luxonEnableCustomLogSink([MarshalAs(UnmanagedType.I1)] bool enable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr getOrCreateLogger(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void setLoggerLevel(IntPtr logger, int level);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void loggerTrace(
        IntPtr logger,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void loggerDebug(
        IntPtr logger,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void loggerInfo(
        IntPtr logger,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void loggerWarn(
        IntPtr logger,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void loggerError(
        IntPtr logger,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void loggerCritical(
        IntPtr logger,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string message
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool peerIsAuthenticated(IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern LuxonServerProtocol peerGetTransportProtocol(IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void peerSend(
        IntPtr peer,
        byte[] payload,
        nuint length,
        byte channel,
        LuxonDeliveryMode delivery_mode
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void peerDisconnect(IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr peerGetUserId(IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr peerGetCurrentGame(IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createGamePeerContainer();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyGamePeerContainer(IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gamePeerIsValid(IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int gamePeerGetActorId(IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gamePeerHasInterestGroup(IntPtr game_peer, byte group);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gamePeerSetInterestGroup(
        IntPtr game_peer,
        byte group,
        [MarshalAs(UnmanagedType.I1)] bool enable
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gamePeerGetInterestGroupsMask(
        IntPtr game_peer,
        uint[] out_mask_words,
        nuint max_words
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gamePeerSetInterestGroupsMask(
        IntPtr game_peer,
        uint[] mask_words,
        nuint word_count
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gamePeerGetActorProps(IntPtr game_peer, IntPtr out_props);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gamePeerSetActorProps(IntPtr game_peer, IntPtr props);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr gamePeerGetBasePeer(IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gamePeerDisconnect(IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createEvent();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyEvent(IntPtr event_handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventSetRoutingMetadata(
        IntPtr event_handle,
        byte code,
        int sender_actor_id,
        LuxonDeliveryMode delivery_mode,
        byte channel,
        byte interest_group
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventGetRoutingMetadata(
        IntPtr event_handle,
        out byte out_code,
        out int out_sender_id,
        out LuxonDeliveryMode out_mode,
        out byte out_channel,
        out byte out_group
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventSetReceiversAll(IntPtr event_handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventSetReceiversGroup(IntPtr event_handle, byte group);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventSetReceiversActors(
        IntPtr event_handle,
        int[] actor_ids,
        nuint count
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern LuxonEventReceiversType eventGetReceiversType(IntPtr event_handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern byte eventGetReceiversGroup(IntPtr event_handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventGetReceiversActors(
        IntPtr event_handle,
        int[] out_actor_ids,
        nuint max_count,
        out nuint out_written
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventSetData(IntPtr event_handle, IntPtr data);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventGetData(IntPtr event_handle, IntPtr out_data);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventSetTopParams(IntPtr event_handle, IntPtr top_params);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void eventGetTopParams(IntPtr event_handle, IntPtr out_top_params);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetId(IntPtr game, byte[] out_id, nuint max_len);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetConfigState(
        IntPtr game,
        out byte out_flags,
        [MarshalAs(UnmanagedType.I1)] out bool out_is_created,
        [MarshalAs(UnmanagedType.I1)] out bool out_is_open,
        [MarshalAs(UnmanagedType.I1)] out bool out_is_visible,
        out byte out_max_peers,
        out int out_master_actor
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetConfigState(
        IntPtr game,
        byte flags,
        [MarshalAs(UnmanagedType.I1)] bool is_open,
        [MarshalAs(UnmanagedType.I1)] bool is_visible,
        byte max_peers,
        int master_actor
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int gameGetMasterActor(IntPtr game);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetMasterActor(IntPtr game, int actor_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern int gameGetLastActorId(IntPtr game);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern byte gameGetMaxPeers(IntPtr game);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetMaxPeers(IntPtr game, byte max_peers);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetTtlConfig(
        IntPtr game,
        out int out_player_ttl,
        out int out_empty_ttl
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetTtlConfig(IntPtr game, int player_ttl, int empty_ttl);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetCustomProps(IntPtr game, IntPtr out_custom_props);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetCustomProps(IntPtr game, IntPtr custom_props);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetLobbyPropsToSerValue(IntPtr game, IntPtr out_keys_array);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetLobbyPropsFromSerValue(IntPtr game, IntPtr keys_array);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetExpectedUsersToSerValue(IntPtr game, IntPtr out_list);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameSetExpectedUsersFromSerValue(IntPtr game, IntPtr list);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameAddExpectedUser(
        IntPtr game,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string user_id
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameRemoveExpectedUser(
        IntPtr game,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string user_id
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern nuint gameGetPeerCount(IntPtr game);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr gameCreatePeer(IntPtr game, IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr gameAddPeer(IntPtr game, IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameRemovePeer(IntPtr game, IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameFloodPeer(IntPtr game, IntPtr game_peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr gameFindPeerByActorId(IntPtr game, int actor_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr gameFindPeerByBasePeer(IntPtr game, IntPtr peer);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameBroadcastEvent(IntPtr game, IntPtr event_handle);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameValidateJoin(
        IntPtr game,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string user_id,
        nuint new_expected_users_count,
        out short out_err_code,
        byte[] out_err_str,
        nuint max_err_len
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameTriggerLobbyUpdate(IntPtr game);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetGameProp(IntPtr game, IntPtr key, IntPtr out_val);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetLobbyGameProps(IntPtr game, IntPtr out_hashtable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetGameProps(
        IntPtr game,
        [MarshalAs(UnmanagedType.I1)] bool no_custom,
        IntPtr out_hashtable
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameGetActorProps(IntPtr game, IntPtr out_hashtable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void gameInsertGameProps(IntPtr game, IntPtr update, bool broadcast);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameExpectGameProps(IntPtr game, IntPtr expected);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameInsertActorProps(
        IntPtr game,
        int actor_id,
        IntPtr update,
        bool broadcast
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameExpectActorProps(IntPtr game, int actor_id, IntPtr expected);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool gameMatchesFilter(IntPtr event_data, IntPtr filter);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createLobby(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        byte type
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyLobby(IntPtr lobby);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr lobbyCreateGame(
        IntPtr lobby,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string id,
        [MarshalAs(UnmanagedType.I1)] bool or_get,
        IntPtr out_err_resp
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern nuint lobbyGetPeerCount(IntPtr lobby);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern nuint lobbyGetMasterPeerCount(IntPtr lobby);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool lobbyQueryLobbies(
        IntPtr lobby,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string sql_queries,
        byte[] out_buf,
        nuint max_len,
        out nuint out_written
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createServerManagerConfig();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyServerManagerConfig(IntPtr config);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr loadServerManagerConfigFromFile(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string config_file_path
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr parseServerManagerConfig(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string yaml_config_contents
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigAddServer(
        IntPtr config,
        LuxonServerType type,
        ushort port
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigAddServerAndEndpoint(
        IntPtr config,
        LuxonServerType type,
        ushort port,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string external_udp_address
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigAddEndpoint(
        IntPtr config,
        LuxonServerType type,
        LuxonServerProtocol protocol,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string address
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigSetLimits(
        IntPtr config,
        [MarshalAs(UnmanagedType.I1)] bool enable_ipv6,
        uint max_connections,
        uint max_game_peers,
        uint tick_time_budget
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigSetSettingsDatabasePath(
        IntPtr config,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigEnableHttp(
        IntPtr config,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string address,
        ushort port
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerConfigDisableHttp(IntPtr config);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createServerManagerFromConfig(IntPtr config);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createServerManagerFromFile(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string config_file_path
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr createServerManagerFromContents(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string yaml_config_contents
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyServerManager(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerRun(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool serverManagerRunOnce(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerStop(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool serverManagerGetEndpointOf(
        IntPtr manager,
        LuxonServerType server_type,
        LuxonServerProtocol server_proto,
        byte[] out_address,
        nuint max_len
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern nuint serverManagerGetConnectionCount(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern uint serverManagerGetMaxConnections(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern byte serverManagerGetMaxGamePeers(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr handlerBaseGetServerManager(IntPtr handler);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr gameGetServerManager(IntPtr game);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool serverManagerShouldAbortActiveCommand(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void serverManagerMarkCommandCommited(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern IntPtr serverManagerTakeCommandRestarter(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void commandRestarterRestart(IntPtr restarter);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void destroyCommandRestarter(IntPtr restarter);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern uint registerGamePlugin(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string plugin_name
    );

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool registerAuthPlugin(uint type);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void registerHookpoints(IntPtr manager);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern void luxonSetServerImports(ref NativeImportsStruct imports);
}
