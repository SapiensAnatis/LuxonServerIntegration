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

    public required Message ManagedMessage { get; init; }

    // Do not dispose CommandRestarter - it is freed by native on calling Restart()
    public required CommandRestarter CommandRestarter { get; init; }
}
