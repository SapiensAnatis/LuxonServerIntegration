using System.Diagnostics;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Helpers;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class Actor : IActor
{
    private readonly GamePeer peer;

    public Actor(GamePeer peer)
    {
        this.peer = peer;
    }

    public int ActorNr => this.peer.ActorId;

    public void SetProperties(IDictionary<object, object?> properties)
    {
        this.peer.SetActorProperties(Value.FromManagedDictionary(properties));
    }

    public IDictionary<object, object?> Properties
    {
        get
        {
            var luxonTable = this.peer.GetActorProperties();
            if (luxonTable is not { Content: IDictionary<Value, Value> hashtable })
            {
                throw new UnreachableException("Actor properties was not a dictionary");
            }

            return new TypelessLuxonDictionaryProxy(hashtable);
        }
    }
}
