using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using SharpYaml.Serialization;

namespace Luxon.Server.Host;

internal sealed class PluginConfigList
{
    public required IReadOnlyList<PluginConfig> Plugins { get; init; }
}

internal sealed class PluginConfig
{
    public required string Name { get; init; }

    public required string Version { get; init; }

    public required string AssemblyName { get; init; }

    public required string Type { get; init; }

    public required IConfigurationSection Configuration { get; init; }
}
