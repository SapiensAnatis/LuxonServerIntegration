using System.Diagnostics;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Helpers;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class Actor : IActor
{
    private readonly Game game;
    private readonly GamePeer gamePeer;

    public Actor(Game game, GamePeer gamePeer)
    {
        this.game = game;
        this.gamePeer = gamePeer;
        this.ActorNr = gamePeer.ActorId;
    }

    public int ActorNr { get; set; }

    public void SetProperties(IDictionary<object, object?> properties)
    {
        this.gamePeer.SetActorProperties(Value.FromManagedHashtable(properties));
    }

    public IDictionary<object, object?> Properties
    {
        get
        {
            var luxonTable = this.gamePeer.GetActorProperties();
            if (luxonTable is not { Content: IDictionary<Value, Value> hashtable })
            {
                throw new UnreachableException("Actor properties was not a dictionary");
            }

            return new TypelessLuxonDictionaryProxy(hashtable);
        }
    }
}
