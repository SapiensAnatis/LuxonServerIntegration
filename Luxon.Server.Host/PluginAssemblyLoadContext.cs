using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace Luxon.Server.Host;

internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext
{
    // TODO: find a better way to determine the plugin load path
    private static readonly string PluginsBasePath = Path.Join(
        AppContext.BaseDirectory,
        "../../../../Luxon.Server.GluonPlugin/bin/Debug/net10.0/"
    );

    private readonly string pluginPath;
    private readonly AssemblyDependencyResolver pluginResolver;

    private static readonly AssemblyDependencyResolver HostResolver = new(
        Assembly.GetExecutingAssembly().Location
    );

    public PluginAssemblyLoadContext(PluginConfig config)
        : base(name: $"PluginLoadContext_{config.Name}", isCollectible: false)
    {
        this.pluginPath = Path.Join(PluginsBasePath, config.AssemblyName);
        this.pluginResolver = new AssemblyDependencyResolver(this.pluginPath);
    }

    public Assembly LoadPlugin()
    {
        return this.LoadFromAssemblyPath(this.pluginPath);
    }

    protected override Assembly? Load(AssemblyName name)
    {
        if (HostResolver.ResolveAssemblyToPath(name) != null)
        {
            // Dependency is provided by hosting environment, e.g. Luxon.Server.Plugin
            return null;
        }

        string? path = this.pluginResolver.ResolveAssemblyToPath(name);

        if (path == null)
        {
            return null;
        }

        return this.LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string name)
    {
        throw new NotSupportedException();
    }
}
