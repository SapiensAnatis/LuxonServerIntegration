namespace Luxon.Server.GluonPlugin.Shared;

/// <summary>
/// Shared instance for immutable configuration to be shared between sub-plugins.
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes. Created via reflection
internal sealed class PluginConfiguration
#pragma warning restore CA1812
{
    public required Uri ApiServerUrl { get; init; }

    public required Uri StateManagerUrl { get; init; }

    public required Uri DungeonRecordMultiEndpoint { get; init; }

    public required Uri TimeAttackEndpoint { get; init; }

    public int ReplayTimeoutSeconds { get; init; }

    public required string BearerToken { get; init; }

    //
    // [Obsolete]
    // public bool EnableSecondaryServer { get; init; }
    //
    // [Obsolete]
    // public long SecondaryViewerIdCriterion { get; init; }
    //
    // [Obsolete]
    // public Uri? SecondaryApiServerUrl { get; init; }
    //
    // [Obsolete]
    // public Uri? SecondaryStateManagerUrl { get; init; }
    //
    // [Obsolete]
    // public string? SecondaryBearerToken { get; init; }
    //
    public int RandomMatchingStartDelayMs { get; init; }

    public bool EnableDiscordIntegration { get; init; }

    public Uri? DiscordUrl { get; init; }

    public string? DiscordBearerToken { get; init; }
}
