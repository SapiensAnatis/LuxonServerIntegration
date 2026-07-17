using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace Luxon.Server.Host;

internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext
{
    // Plugins live in a stable, versioned layout next to the host:
    //   plugins/<Name>/<Version>/<AssemblyName>
    // In development the build stages the sibling plugin project into the same
    // layout (see the StagePlugins target in Luxon.Server.Host.csproj), so this
    // resolution rule is identical for dev and prod.
    private static readonly string PluginsRoot = Path.Combine(AppContext.BaseDirectory, "plugins");

    private readonly string pluginPath;
    private readonly AssemblyDependencyResolver pluginResolver;

    private static readonly AssemblyDependencyResolver HostResolver = new(
        Assembly.GetExecutingAssembly().Location
    );

    public PluginAssemblyLoadContext(PluginConfig config)
        : base(name: $"PluginLoadContext_{config.Name}", isCollectible: false)
    {
        string pluginDir = Path.Combine(PluginsRoot, config.Name, config.Version);
        this.pluginPath = Path.Combine(pluginDir, config.AssemblyName);

        if (!File.Exists(this.pluginPath))
        {
            throw new FileNotFoundException(
                $"Plugin '{config.Name}' assembly not found at '{this.pluginPath}'. "
                    + $"Expected layout: plugins/{config.Name}/{config.Version}/{config.AssemblyName}",
                this.pluginPath
            );
        }

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
