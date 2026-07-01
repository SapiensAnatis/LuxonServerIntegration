namespace Luxon.Server.Plugin;

public interface ILeaveGameCallInfo : ITypedCallInfo<ILeaveGameRequest>
{
    /// <summary>
    /// Gets the actor number of the actor leaving the room.
    /// </summary>
    int ActorNr { get; }

    /// <summary>
    /// Gets a code for the reason why the actor is leaving the room.
    /// </summary>
    int Reason { get; }
}
