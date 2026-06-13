using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Plugin;

public interface IPluginFactory
{
    GamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config);
}
