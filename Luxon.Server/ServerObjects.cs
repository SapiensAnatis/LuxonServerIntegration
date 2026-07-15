using Luxon.Server.Interop;

namespace Luxon.Server;

public sealed class CommandRestarter : NativeResource
{
    internal CommandRestarter(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle) { }

    public void Restart()
    {
        // commandRestarterRestart takes ownership of the handle and destroys it unconditionally, even if the restart
        // fails. Detach first so ownership is released regardless of whether Checked throws; otherwise the finalizer
        // would double-free the native object.
        var handle = DetachHandle();
        LuxonInteropHelpers.Checked(() => NativeMethods.commandRestarterRestart(handle));
    }

    protected override void ReleaseHandle(IntPtr handle) =>
        NativeMethods.destroyCommandRestarter(handle);
}

public sealed class ServerManagerConfig : NativeResource
{
    internal ServerManagerConfig(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle) { }

    public static ServerManagerConfig Create() =>
        new(LuxonInteropHelpers.Checked(NativeMethods.createServerManagerConfig), true);

    public static ServerManagerConfig LoadFromFile(string configFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(configFilePath);
        return new ServerManagerConfig(
            LuxonInteropHelpers.Checked(() =>
                NativeMethods.loadServerManagerConfigFromFile(configFilePath)
            ),
            true
        );
    }

    public static ServerManagerConfig Parse(string yamlConfigContents)
    {
        ArgumentNullException.ThrowIfNull(yamlConfigContents);
        return new ServerManagerConfig(
            LuxonInteropHelpers.Checked(() =>
                NativeMethods.parseServerManagerConfig(yamlConfigContents)
            ),
            true
        );
    }

    public ServerManagerConfig AddServer(LuxonServerType type, ushort port)
    {
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerConfigAddServer(Handle, type, port)
        );
        return this;
    }

    public ServerManagerConfig AddServerAndEndpoint(
        LuxonServerType type,
        ushort port,
        string externalUdpAddress
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(externalUdpAddress);
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerConfigAddServerAndEndpoint(
                Handle,
                type,
                port,
                externalUdpAddress
            )
        );
        return this;
    }

    public ServerManagerConfig AddEndpoint(
        LuxonServerType type,
        LuxonServerProtocol protocol,
        string address
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(address);
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerConfigAddEndpoint(Handle, type, protocol, address)
        );
        return this;
    }

    public ServerManagerConfig SetLimits(
        bool enableIpv6,
        uint maxConnections,
        uint maxGamePeers,
        uint tickTimeBudget
    )
    {
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerConfigSetLimits(
                Handle,
                enableIpv6,
                maxConnections,
                maxGamePeers,
                tickTimeBudget
            )
        );
        return this;
    }

    public ServerManagerConfig SetSettingsDatabasePath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerConfigSetSettingsDatabasePath(Handle, path)
        );
        return this;
    }

    public ServerManagerConfig EnableHttp(string address, ushort port)
    {
        ArgumentException.ThrowIfNullOrEmpty(address);
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerConfigEnableHttp(Handle, address, port)
        );
        return this;
    }

    public ServerManagerConfig DisableHttp()
    {
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerConfigDisableHttp(Handle));
        return this;
    }

    protected override void ReleaseHandle(IntPtr handle) =>
        NativeMethods.destroyServerManagerConfig(handle);
}

public sealed class ServerManager : NativeResource
{
    internal ServerManager(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle) { }

    public static ServerManager CreateFromConfig(ServerManagerConfig config)
    {
        LuxonInteropHelpers.ThrowIfDisposed(config, nameof(config));
        // Function takes ownership of handle
        IntPtr handle = config.DetachHandle();
        return new ServerManager(
            LuxonInteropHelpers.Checked(() => NativeMethods.createServerManagerFromConfig(handle)),
            true
        );
    }

    public static ServerManager CreateFromFile(string configFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(configFilePath);
        return new ServerManager(
            LuxonInteropHelpers.Checked(() =>
                NativeMethods.createServerManagerFromFile(configFilePath)
            ),
            true
        );
    }

    public static ServerManager CreateFromContents(string yamlConfigContents)
    {
        ArgumentNullException.ThrowIfNull(yamlConfigContents);
        return new ServerManager(
            LuxonInteropHelpers.Checked(() =>
                NativeMethods.createServerManagerFromContents(yamlConfigContents)
            ),
            true
        );
    }

    public nuint ConnectionCount =>
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerGetConnectionCount(Handle));

    public uint MaxConnections =>
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerGetMaxConnections(Handle));

    public byte MaxGamePeers =>
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerGetMaxGamePeers(Handle));

    public void Run() => LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerRun(Handle));

    public bool RunOnce() =>
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerRunOnce(Handle));

    public void Stop() =>
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerStop(Handle));

    public string? GetEndpointOf(
        LuxonServerType serverType,
        LuxonServerProtocol protocol,
        int bufferSize = 2048
    )
    {
        if (bufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        var buffer = new byte[bufferSize];
        var found = LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerGetEndpointOf(
                Handle,
                serverType,
                protocol,
                buffer,
                (nuint)buffer.Length
            )
        );
        return found ? LuxonInteropHelpers.DecodeUtf8(buffer) : null;
    }

    public bool TryGetEndpointOf(
        LuxonServerType serverType,
        LuxonServerProtocol protocol,
        out string? address,
        int bufferSize = 2048
    )
    {
        address = GetEndpointOf(serverType, protocol, bufferSize);
        return address is not null;
    }

    public bool ShouldAbortActiveCommand() =>
        LuxonInteropHelpers.Checked(() =>
            NativeMethods.serverManagerShouldAbortActiveCommand(Handle)
        );

    public void MarkCommandCommitted() =>
        LuxonInteropHelpers.Checked(() => NativeMethods.serverManagerMarkCommandCommited(Handle));

    public CommandRestarter TakeCommandRestarter()
    {
        try
        {
            return new CommandRestarter(
                LuxonInteropHelpers.Checked(() =>
                    NativeMethods.serverManagerTakeCommandRestarter(Handle)
                ),
                true
            );
        }
        catch (EntryPointNotFoundException ex)
        {
            throw new LuxonFeatureUnavailableException("command restarter", ex);
        }
    }

    public void RegisterHookpoints() => LuxonServerRuntime.RegisterHookpoints(this);

    protected override void ReleaseHandle(IntPtr handle) =>
        NativeMethods.destroyServerManager(handle);
}
