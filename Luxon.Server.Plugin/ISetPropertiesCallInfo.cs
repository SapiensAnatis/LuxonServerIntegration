namespace Luxon.Server.Plugin;

public interface ISetPropertiesCallInfo : ITypedCallInfo<ISetPropertiesRequest>
{
    /// <summary>
    /// The number of the actor triggering the current callback.
    /// </summary>
    int ActorNr { get; }
}
