using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Luxon.Server.Plugin;
using SharpYaml;

namespace Luxon.Server.Host;

internal sealed class PluginManager
{
    private readonly Dictionary<uint, PluginDetails> pluginIdToPluginMap = [];

    public void LoadPluginsFromConfig(string pluginConfigPath)
    {
        List<PluginConfig>? plugins;

        using (FileStream fs = File.OpenRead(pluginConfigPath))
        {
            plugins = YamlSerializer.Deserialize<List<PluginConfig>>(fs);
        }

        if (plugins is not { Count: > 0 })
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
                    $"Failed to load type {plugin.Type} from assembly {plugin.AssemblyName}: not found"
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

            Debug.Assert(id != 0);

            this.pluginIdToPluginMap.Add(id, new PluginDetails(plugin.Name, factory, loadContext));
        }
    }

    public bool TryCreatePlugin(uint id, [NotNullWhen(true)] out IGamePlugin? plugin)
    {
        plugin = null;
        if (!this.pluginIdToPluginMap.TryGetValue(id, out PluginDetails? pluginDetails))
        {
            return false;
        }

        plugin = pluginDetails.Factory.Create(null, pluginDetails.Name, null);
        return true;
    }

    private sealed record PluginDetails(
        string Name,
        IPluginFactory Factory,
        PluginAssemblyLoadContext AssemblyLoadContext
    );
}
