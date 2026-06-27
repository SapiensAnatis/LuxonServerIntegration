using Microsoft.Extensions.Logging;

namespace Luxon.Server.Plugin;

public interface IPluginHost
{
    public string GameId { get; }

    public ILogger<T> CreateLogger<T>();
}
