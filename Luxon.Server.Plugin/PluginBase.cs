using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Plugin;

public abstract class PluginBase : IGamePlugin
{
    protected PluginBase(IPluginHost host)
    {
        this.PluginHost = host;
    }

    protected IPluginHost PluginHost { get; set; }

    public virtual Task OnCreateGame(ICreateGameCallInfo info, CancellationToken cancellationToken)
    {
#pragma warning disable CA1062
        info.Continue();
#pragma warning restore CA1062

        return Task.CompletedTask;
    }

    public virtual void BeforeJoinGame() { }

    public virtual void OnJoinGame() { }
}
