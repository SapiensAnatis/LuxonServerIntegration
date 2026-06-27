using System.Diagnostics.CodeAnalysis;
using SharpYaml.Serialization;

namespace Luxon.Server.Host;

[SuppressMessage(
    "Microsoft.Performance",
    "CA1812:AvoidUninstantiatedInternalClasses",
    Justification = "Class is instantiated from YamlSerializer"
)]
// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class PluginConfig
{
    [YamlRequired]
    public required string Name { get; init; }

    [YamlRequired]
    public required string Version { get; init; }

    [YamlRequired]
    public required string AssemblyName { get; init; }

    [YamlRequired]
    public required string Type { get; init; }
}
