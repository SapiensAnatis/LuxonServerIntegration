using System.Runtime.InteropServices;
using Luxon.Ser;
using Luxon.Server.Host.Helpers;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host.Plugin;

internal sealed class PluginHost(Game game) : IPluginHost
{
    public string GameId => game.Id;

    public List<Actor> GameActorsInternal { get; } = [];

    public IReadOnlyList<IActor> GameActors => this.GameActorsInternal.AsReadOnly();

    public ILogger<IPluginHost> Logger { get; } =
        LoggerConfiguration.Factory.CreateLogger<IPluginHost>();

    public IDictionary<object, object?> GameProperties { get; set; } =
        new Dictionary<object, object?>();

    public void BroadcastEvent(
        byte targetGroup,
        int senderActor,
        byte interestGroup,
        byte evCode,
        IDictionary<byte, object?> data,
        SendParameters sendParameters = new()
    )
    {
        using var evt = LuxonEvent.Create();
        evt.SetReceiversGroup(targetGroup);
        evt.SetRoutingMetadata(
            evCode,
            senderActor,
            sendParameters.DeliveryMode,
            sendParameters.ChannelId,
            interestGroup
        );
        evt.SetData(MakeLuxonDictionary(data));

        game.BroadcastEvent(evt);
    }

    public void BroadcastEvent(
        ReadOnlySpan<int> receiverActors,
        int senderActor,
        byte evCode,
        IDictionary<byte, object?> data,
        SendParameters sendParameters = new()
    )
    {
        const int interestGroupAllUsers = 0;

        using var evt = LuxonEvent.Create();
        evt.SetReceiversActors(receiverActors);
        evt.SetRoutingMetadata(
            evCode,
            senderActor,
            sendParameters.DeliveryMode,
            sendParameters.ChannelId,
            interestGroupAllUsers
        );
        evt.SetData(MakeLuxonDictionary(data));

        game.BroadcastEvent(evt);
    }

    public ILogger<T> CreateLogger<T>()
    {
        return LoggerConfiguration.Factory.CreateLogger<T>();
    }

    private static Value MakeLuxonDictionary(IDictionary<byte, object?> managedDictionary)
    {
        var dict = managedDictionary.ToDictionary(
            x => Value.FromManaged(x.Key),
            x => Value.FromManaged(x.Value)
        );

        return Value.FromManaged(dict);
    }
}
