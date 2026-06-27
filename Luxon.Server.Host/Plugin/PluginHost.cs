using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host.Plugin;

internal sealed class PluginHost : IPluginHost
{
    public required string GameId { get; init; }

    public List<Actor> GameActorsInternal { get; } = [];

    public IReadOnlyList<IActor> GameActors => this.GameActorsInternal.AsReadOnly();

    public ILogger<T> CreateLogger<T>()
    {
        return LoggerConfiguration.Factory.CreateLogger<T>();
    }
}
