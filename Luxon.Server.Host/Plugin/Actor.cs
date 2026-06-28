using System.Diagnostics;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Helpers;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class Actor : IActor
{
    private Game game;
    private bool gamePeerStale;

    private GamePeer GamePeer
    {
        get
        {
            if (this.gamePeerStale)
            {
                field =
                    this.game.FindPeerByActorId(this.ActorNr)
                    ?? throw new InvalidOperationException($"Actor {this.ActorNr} not found");

                this.gamePeerStale = false;
            }

            return field;
        }
        set;
    }

    public Actor(Game game, GamePeer gamePeer)
    {
        this.game = game;
        this.GamePeer = gamePeer;
        this.ActorNr = gamePeer.ActorId;
        this.gamePeerStale = false;
    }

    public int ActorNr { get; set; }

    public void SetProperties(IDictionary<object, object?> properties)
    {
        this.GamePeer.SetActorProperties(Value.FromManagedDictionary(properties));
    }

    public void InvalidateGamePeer()
    {
        this.gamePeerStale = true;
    }

    public IDictionary<object, object?> Properties
    {
        get
        {
            var luxonTable = this.GamePeer.GetActorProperties();
            if (luxonTable is not { Content: IDictionary<Value, Value> hashtable })
            {
                throw new UnreachableException("Actor properties was not a dictionary");
            }

            return new TypelessLuxonDictionaryProxy(hashtable);
        }
    }
}
