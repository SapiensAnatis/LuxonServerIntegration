using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Plugin;

public interface IPluginFactory
{
    IGamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config);
}
