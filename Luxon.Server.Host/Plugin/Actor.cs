using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class Actor : IActor
{
    public Actor() { }

    public Actor(GamePeer peer)
    {
        this.ActorNr = peer.ActorId;
    }

    public int ActorNr { get; init; }
}
