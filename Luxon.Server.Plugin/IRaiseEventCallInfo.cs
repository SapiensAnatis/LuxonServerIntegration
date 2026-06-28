namespace Luxon.Server.Plugin;

public interface IRaiseEventCallInfo : ICallInfo, ITypedCallInfo<IRaiseEventRequest>
{
    int ActorNr { get; }
}
