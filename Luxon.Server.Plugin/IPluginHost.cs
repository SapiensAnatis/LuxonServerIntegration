using Microsoft.Extensions.Logging;

namespace Luxon.Server.Plugin;

public interface IPluginHost
{
    string GameId { get; }

    ILogger<T> CreateLogger<T>();

    public IReadOnlyList<IActor> GameActors { get; }
}
