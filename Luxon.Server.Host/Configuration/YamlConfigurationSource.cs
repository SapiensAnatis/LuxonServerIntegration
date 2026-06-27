using Microsoft.Extensions.Configuration;

namespace Luxon.Server.Host.Configuration;

internal sealed class YamlConfigurationSource(string filePath) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new YamlConfigurationProvider(filePath);
    }
}
