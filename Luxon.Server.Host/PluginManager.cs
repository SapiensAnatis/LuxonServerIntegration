using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Luxon.Server.Plugin;
using SharpYaml;

namespace Luxon.Server.Host;

internal class PluginManager
{
    private readonly Dictionary<uint, PluginDetails> _pluginIdToPluginMap = [];

    public void LoadPluginsFromConfig(string pluginConfigPath)
    {
        List<PluginConfig>? plugins;

        using (FileStream fs = File.OpenRead(pluginConfigPath))
        {
            plugins = YamlSerializer.Deserialize<List<PluginConfig>>(fs);
        }

        // TODO: find a better way to determine the plugin load path
        string pluginsBasePath = Path.Join(
            AppContext.BaseDirectory,
            "../../../../Luxon.Server.GluonPlugin/bin/Debug/net10.0/"
        );

        if (plugins is not { Count: > 0 })
        {
            return;
        }

        foreach (var plugin in plugins)
        {
            string pluginPath = Path.Join(pluginsBasePath, plugin.AssemblyName);

            var loadContext = new PluginLoadContext(pluginPath);

            Assembly pluginAssembly = loadContext.LoadFromAssemblyPath(pluginPath);
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

            _pluginIdToPluginMap.Add(id, new PluginDetails(plugin.Name, factory, loadContext));
        }
    }

    public bool TryCreatePlugin(uint id, [NotNullWhen(true)] out GamePlugin? plugin)
    {
        plugin = null;
        if (!_pluginIdToPluginMap.TryGetValue(id, out PluginDetails? pluginDetails))
        {
            return false;
        }

        plugin = pluginDetails.Factory.Create(null, pluginDetails.Name, null);
        return true;
    }

    private record PluginDetails(
        string Name,
        IPluginFactory Factory,
        PluginLoadContext LoadContext
    );
}
