namespace Luxon.Server.Plugin;

public interface IActor
{
    int ActorNr { get; }

    IDictionary<object, object?> Properties { get; }
}
