using Microsoft.Extensions.Logging;

namespace Luxon.Server.Plugin;

public interface IPluginHost
{
    string GameId { get; }

    ILogger<T> CreateLogger<T>();

    IReadOnlyList<IActor> GameActors { get; }

    ILogger<IPluginHost> Logger { get; }

    IDictionary<object, object?> GameProperties { get; }

    public void BroadcastEvent(
        byte target,
        int senderActor,
        byte targetGroup,
        byte evCode,
        IDictionary<byte, object> data,
        byte cacheOp
    );

    public void BroadcastEvent(
        IList<int> receiverActors,
        int senderActor,
        byte evCode,
        IDictionary<byte, object> data,
        byte cacheOp
    );
}
