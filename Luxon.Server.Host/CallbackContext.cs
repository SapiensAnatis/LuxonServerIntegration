using Luxon.Ser;
using Luxon.Server.Host.Plugin;

namespace Luxon.Server.Host;

internal class TaskContext : IDisposable
{
    public required Task PluginTask { get; init; }

    public required CancellationTokenSource CancellationTokenSource { get; init; }

    public virtual void Dispose()
    {
        this.CancellationTokenSource.Dispose();
    }
}

internal sealed class InfoCallbackContext : TaskContext
{
    public required CallInfo CallInfo { get; init; }

    public required SerMessageRef? Request { get; init; }

    public required Message? ManagedRequest { get; init; }

    public required CoroutineHandle CoroutineHandle { get; init; }

    public override void Dispose()
    {
        base.Dispose();
        this.CoroutineHandle.Dispose();
    }
}
