using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Luxon.Server.Host.Plugin;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpYaml;

namespace Luxon.Server.Host;

internal sealed partial class PluginManager : IDisposable
{
    private static readonly ILogger<PluginManager> Logger =
        LoggerConfiguration.Factory.CreateLogger<PluginManager>();

    private readonly IConfigurationRoot pluginConfiguration;
    private readonly Dictionary<uint, PluginDetails> pluginIdToPluginMap;
    private readonly SocketsHttpHandler httpHandler;

    public PluginManager(IConfigurationRoot pluginConfiguration)
    {
        this.pluginConfiguration = pluginConfiguration;
        this.pluginIdToPluginMap = [];

        this.httpHandler = new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
    }

    public void LoadPluginsFromConfig()
    {
        var config = this.pluginConfiguration.Get<PluginConfigList>();

        if (config is not { Plugins: { Count: > 0 } plugins })
        {
            return;
        }

        foreach (var plugin in plugins)
        {
            var loadContext = new PluginAssemblyLoadContext(plugin);

            Assembly pluginAssembly = loadContext.LoadPlugin();

            Type? pluginType = pluginAssembly.GetType(plugin.Type);

            if (pluginType is null)
            {
                throw new InvalidOperationException(
                    $"Failed to load type {plugin.Type} from assembly {plugin.AssemblyName}: type not found"
                );
            }

            if (!pluginType.IsAssignableTo(typeof(IPluginFactory)))
            {
                throw new InvalidOperationException(
                    $"Loaded plugin factory {pluginType} is not assignable to IPluginFactory"
                );
            }

            IPluginFactory? factory = (IPluginFactory?)Activator.CreateInstance(pluginType);

            if (factory is null)
            {
                throw new InvalidOperationException(
                    $"Failed to create an instance of type {plugin.Type}"
                );
            }

            uint id = LuxonServerRuntime.RegisterGamePlugin(plugin.Name);

            if (id == 0)
            {
                throw new InvalidOperationException(
                    "Native Luxon method rejected plugin registration"
                );
            }

            this.pluginIdToPluginMap.Add(
                id,
                new PluginDetails(plugin.Name, factory, loadContext, plugin.Configuration)
            );

            LogPluginLoaded(Logger, plugin.Name, plugin.Version);
        }
    }

    [LoggerMessage(LogLevel.Information, "Loaded plugin {PluginName} ({PluginVersion})")]
    private static partial void LogPluginLoaded(
        ILogger logger,
        string pluginName,
        string pluginVersion
    );

    public bool TryCreatePlugin(uint id, Game game, [NotNullWhen(true)] out IGamePlugin? plugin)
    {
        plugin = null;
        if (!this.pluginIdToPluginMap.TryGetValue(id, out PluginDetails? pluginDetails))
        {
            return false;
        }

        PluginHost host = new(game, new HttpClient(this.httpHandler));

        plugin = pluginDetails.Factory.Create(
            host,
            pluginDetails.Name,
            pluginDetails.Configuration
        );

        return true;
    }

    private sealed record PluginDetails(
        string Name,
        IPluginFactory Factory,
        PluginAssemblyLoadContext AssemblyLoadContext,
        IConfiguration Configuration
    );

    public void Dispose()
    {
        this.httpHandler.Dispose();
    }
}
