namespace Luxon.Server.Plugin;

public interface ISetPropertiesRequest
{
    int ActorNumber { get; }

    IDictionary<object, object?> Properties { get; }
}
