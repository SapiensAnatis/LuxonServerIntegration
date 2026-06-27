using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Luxon.Server.Host.Plugin;
using Luxon.Server.Plugin;
using Microsoft.Extensions.Configuration;
using SharpYaml;

namespace Luxon.Server.Host;

internal sealed class PluginManager(IConfigurationRoot pluginConfiguration)
{
    private readonly Dictionary<uint, PluginDetails> pluginIdToPluginMap = [];

    public void LoadPluginsFromConfig()
    {
        var config = pluginConfiguration.Get<PluginConfigList>();

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
        }
    }

    public bool TryCreatePlugin(uint id, Game game, [NotNullWhen(true)] out IGamePlugin? plugin)
    {
        plugin = null;
        if (!this.pluginIdToPluginMap.TryGetValue(id, out PluginDetails? pluginDetails))
        {
            return false;
        }

        PluginHost host = new() { GameId = game.Id };

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
}
