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
        byte targetGroup,
        int senderActor,
        byte interestGroup,
        byte evCode,
        IDictionary<byte, object?> data,
        SendParameters sendParameters = new()
    );

    public void BroadcastEvent(
        ReadOnlySpan<int> receiverActors,
        int senderActor,
        byte evCode,
        IDictionary<byte, object?> data,
        SendParameters sendParameters = new()
    );

    public bool SetProperties(
        int actorNr,
        Dictionary<object, object?> data,
        Dictionary<object, object?>? expected,
        bool broadcast
    );
}
