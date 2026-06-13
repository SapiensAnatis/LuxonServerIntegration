using Luxon.Server.Plugin;

namespace Luxon.Server.Host;

internal sealed class GameAdditionalFields
{
    public Dictionary<uint, IGamePlugin> Plugins { get; } = [];
}
