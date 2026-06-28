using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using SharpYaml.Serialization;

namespace Luxon.Server.Host;

[SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Class is instantiated by configuration binding"
)]
internal sealed class PluginConfigList
{
    public required IReadOnlyList<PluginConfig> Plugins { get; init; }
}

[SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Class is instantiated by configuration binding"
)]
internal sealed class PluginConfig
{
    public required string Name { get; init; }

    public required string Version { get; init; }

    public required string AssemblyName { get; init; }

    public required string Type { get; init; }

    public required IConfigurationSection Configuration { get; init; }
}
