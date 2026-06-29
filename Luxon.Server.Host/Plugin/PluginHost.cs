using System.Runtime.InteropServices;
using Luxon.Ser;
using Luxon.Server.Host.Helpers;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host.Plugin;

internal sealed class PluginHost : IPluginHost
{
    private readonly Game game;

    public PluginHost(Game game, HttpClient httpClient)
    {
        this.game = game;
        this.HttpClient = httpClient;
    }

    public string GameId => this.game.Id;

    public IReadOnlyList<IActor> GameActors
    {
        get
        {
            // TODO: this breaks if users leave the game
            List<IActor> actors = [];
            for (nuint i = 1; i <= this.game.PeerCount; i++)
            {
                if (this.game.FindPeerByActorId((int)i) is { } peer)
                {
                    actors.Add(new Actor(this.game, peer));
                }
            }

            return actors;
        }
    }

    public ILogger<IPluginHost> Logger { get; } =
        LoggerConfiguration.Factory.CreateLogger<IPluginHost>();

    public HttpClient HttpClient { get; }

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
        evt.SetTopParams(Value.FromManagedDictionary(data));

        this.game.BroadcastEvent(evt);
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
        evt.SetTopParams(Value.FromManagedDictionary(data));

        this.game.BroadcastEvent(evt);
    }

    public bool SetProperties(
        int actorNr,
        Dictionary<object, object?> data,
        Dictionary<object, object?>? expected,
        bool broadcast
    )
    {
        if (expected != null)
        {
            throw new NotSupportedException(
                "Check-and-swap is not yet supported in SetProperties. Set expected to null to proceed."
            );
        }

        if (actorNr == 0)
        {
            this.game.InsertGameProperties(Value.FromManagedHashtable(data), broadcast);
            return true;
        }

        IActor? actor = this.GameActors.FirstOrDefault((a) => a.ActorNr == actorNr);
        if (actor == null)
        {
            throw new ArgumentException($"Actor {actorNr} not found.", nameof(actorNr));
        }

        actor.SetProperties(data);
        return true;
    }

    public ILogger<T> CreateLogger<T>()
    {
        return LoggerConfiguration.Factory.CreateLogger<T>();
    }
}
