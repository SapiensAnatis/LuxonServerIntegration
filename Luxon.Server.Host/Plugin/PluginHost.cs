using Luxon.Server.Plugin;
using Microsoft.Extensions.Logging;

namespace Luxon.Server.Host.Plugin;

internal sealed class PluginHost : IPluginHost
{
    public required string GameId { get; init; }

    private static LoggerFactory LoggerFactory { get; } = new();

    public ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.CreateLogger<T>();
    }
}
