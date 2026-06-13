using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;

namespace Luxon.Server.GluonPlugin;

public class GluonPluginFactory : IPluginFactory
{
    public IGamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config)
    {
        Console.WriteLine("GluonPlugin: Create");
        return new GluonPlugin();
    }
}
