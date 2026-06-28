namespace Luxon.Server.Plugin;

public interface IBeforeSetPropertiesCallInfo : ITypedCallInfo<ISetPropertiesRequest>
{
    int ActorNr { get; }
}
