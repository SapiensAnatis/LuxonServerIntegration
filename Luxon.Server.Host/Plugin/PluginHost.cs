using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host.Plugin;

internal sealed class PluginHost : IPluginHost
{
    public required string GameId { get; init; }

    public List<Actor> GameActorsInternal { get; } = [];

    public IReadOnlyList<IActor> GameActors => this.GameActorsInternal.AsReadOnly();

    public ILogger<IPluginHost> Logger { get; } =
        LoggerConfiguration.Factory.CreateLogger<IPluginHost>();

    public IDictionary<object, object?> GameProperties { get; set; } =
        new Dictionary<object, object?>();

    public void BroadcastEvent(
        byte target,
        int senderActor,
        byte targetGroup,
        byte evCode,
        IDictionary<byte, object> data,
        byte cacheOp
    )
    {
        throw new NotImplementedException();
    }

    public void BroadcastEvent(
        IList<int> receiverActors,
        int senderActor,
        byte evCode,
        IDictionary<byte, object> data,
        byte cacheOp
    )
    {
        throw new NotImplementedException();
    }

    public ILogger<T> CreateLogger<T>()
    {
        return LoggerConfiguration.Factory.CreateLogger<T>();
    }
}
