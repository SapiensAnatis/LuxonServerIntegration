# Luxon.Server API Summary

## Starting the Server

The main entry point is `ServerManager`. There are three ways to create one:

```csharp
// 1. From a config file (YAML)
using var manager = ServerManager.CreateFromFile("config.yaml");

// 2. From a YAML string
using var manager = ServerManager.CreateFromContents(yamlString);

// 3. From a programmatic config object
using var config = ServerManagerConfig.Create()
    .AddServer(LuxonServerType.MasterServer, port: 5055)
    .AddServer(LuxonServerType.GameServer, port: 5056)
    .SetLimits(enableIpv6: false, maxConnections: 1000, maxGamePeers: 32, tickTimeBudget: 100)
    .SetSettingsDatabasePath("settings.db")
    .EnableHttp("0.0.0.0", 8080);

using var manager = ServerManager.CreateFromConfig(config);
```

Then run the server — blocking or tick-by-tick:

```csharp
manager.Run();       // blocks until stopped
// -- or --
while (manager.RunOnce()) { /* custom loop */ }
manager.Stop();      // signal stop from another thread
```

---

## Key APIs

### `LuxonServerRuntime` (static)
- `SetImports(LuxonServerImports)` — register all plugin/hook callbacks before starting
- `RegisterGamePlugin(string)` → `uint pluginId`
- `RegisterAuthPlugin(uint type)`
- `RegisterHookpoints(ServerManager)`
- `EnableCustomLogSink(bool)` — redirect logs to `CustomLogSink` callback
- `UseCustomNativeLibraryPath(string)` — override native `.so`/`.dll` path
- `UnhandledCallbackException` event — catch exceptions thrown inside callbacks

### `LuxonServerImports` (callback registration)
All fields are optional delegates wired into the native server:

| Callback | When it fires |
|---|---|
| `GamePluginOnAttach` | Plugin attached to a game |
| `GamePluginOnCreateGame` | Game creation request |
| `GamePluginBeforeJoin` / `OnJoinGame` | Before/after peer joins |
| `GamePluginOnLeave` | Peer leaves |
| `GamePluginOnRaiseEvent` | Event raised in-game |
| `GamePluginBefore/OnSetProperties` | Property update hooks |
| `GamePluginBefore/OnCloseGame` | Game closing |
| `AuthPluginOnAuthenticate` | Custom authentication |
| `CustomLogSink` | Intercept log messages |
| `HookpointMasterServer*` | Low-level master server overrides |

### `ServerManagerConfig` (fluent builder)
- `AddServer(type, port)` — add a NameServer / MasterServer / GameServer
- `AddServerAndEndpoint(type, port, externalUdpAddress)`
- `AddEndpoint(type, protocol, address)`
- `SetLimits(ipv6, maxConn, maxPeers, tickBudget)`
- `SetSettingsDatabasePath(path)`
- `Enable/DisableHttp(address, port)`

### `Game`
Properties: `Id`, `ConfigState`, `MaxPeers`, `MasterActor`, `TtlConfig`, `CustomProperties`, `PeerCount`, `ServerManager`

Methods: `CreatePeer`, `AddPeer`, `RemovePeer`, `FindPeerByActorId`, `BroadcastEvent`, `ValidateJoin`, `TriggerLobbyUpdate`, property get/set helpers

### `Lobby`
- `Lobby.Create(name, type)` — create a lobby
- `CreateGame(id)` / `TryCreateGame(id, out result)` — spawn a game room
- `QueryLobbies(sql)` — SQL-based lobby query

### `Logger`
- `Logger.GetOrCreate(name)` — named logger
- Methods: `Trace/Debug/Info/Warn/Error/Critical(message)`

### `LuxonSerBridge` (serialization helpers)
- `SerializeMessage` / `DeserializeMessage` — `Luxon.Ser.Message` ↔ `byte[]`
- `SerializeValue` / `DeserializeValue` — `Luxon.Ser.Value` ↔ `byte[]`

---

## Minimal Startup Pattern

```csharp
using var reg = LuxonServerRuntime.SetImports(new LuxonServerImports
{
    GamePluginOnJoinGame = (pluginId, game, request, joiner) => LuxonGamePluginResult.Continue,
    // ... other callbacks
});

LuxonServerRuntime.RegisterGamePlugin("MyPlugin");

using var manager = ServerManager.CreateFromFile("luxon.yaml");
manager.Run();
```
