using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Plugin;

public abstract class PluginBase : IGamePlugin
{
    protected PluginBase(IPluginHost host)
    {
        this.PluginHost = host;
    }

    public IPluginHost PluginHost { get; private set; }

    public virtual async Task OnCreateGame(
        ICreateGameCallInfo info,
        CancellationToken cancellationToken
    )
    {
#pragma warning disable CA1062
        info.Continue();
#pragma warning restore CA1062
    }

    public virtual async Task OnJoin(IJoinGameCallInfo info, CancellationToken cancellationToken)
    {
#pragma warning disable CA1062
        info.Continue();
#pragma warning restore CA1062
    }

    public virtual async Task OnLeave(ILeaveGameCallInfo info, CancellationToken cancellationToken)
    {
#pragma warning disable CA1062
        info.Continue();
#pragma warning restore CA1062
    }

    public virtual async Task OnRaiseEvent(
        IRaiseEventCallInfo info,
        CancellationToken cancellationToken
    )
    {
#pragma warning disable CA1062
        info.Continue();
#pragma warning restore CA1062
    }

    public virtual async Task BeforeSetProperties(
        IBeforeSetPropertiesCallInfo info,
        CancellationToken cancellationToken
    )
    {
#pragma warning disable CA1062
        info.Continue();
#pragma warning restore CA1062
    }
}
