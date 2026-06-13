using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;

namespace Luxon.Server.GluonPlugin;

public class GluonPluginFactory : IPluginFactory
{
    public GamePlugin Create(IPluginHost gameHost, string pluginName, IConfiguration config)
    {
        Console.WriteLine("Hello world from GluonPlugin");
        return new GluonPlugin();
    }
}
