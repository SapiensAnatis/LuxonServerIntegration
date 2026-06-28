using System.Diagnostics;
using Luxon.Ser;
using Luxon.Server.Host.Collections;
using Luxon.Server.Host.Helpers;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class GameActor : IActor
{
    public GamePeer GamePeer { get; set; }

    public GameActor(GamePeer GamePeer)
    {
        this.GamePeer = GamePeer;
    }

    public int ActorNr => this.GamePeer.ActorId;

    public void SetProperties(IDictionary<object, object?> properties)
    {
        this.GamePeer.SetActorProperties(Value.FromManagedDictionary(properties));
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
