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
    private readonly AssemblyDependencyResolver resolver;

    public PluginAssemblyLoadContext(PluginConfig config)
        : base(name: $"PluginLoadContext_{config.Name}", isCollectible: false)
    {
        this.pluginPath = Path.Join(PluginsBasePath, config.AssemblyName);
        this.resolver = new AssemblyDependencyResolver(this.pluginPath);
    }

    public Assembly LoadPlugin()
    {
        return this.LoadFromAssemblyPath(this.pluginPath);
    }

    protected override Assembly? Load(AssemblyName name)
    {
        if (name.Name == "Luxon.Server.Plugin")
        {
            // Load the same Luxon.Server.Plugin assembly as the host app, so that checks like
            // pluginType.IsAssignableTo(typeof(IPluginFactory)) actually work. Returning null forwards
            // to the default resolver.
            return null;
        }

        string? path = this.resolver.ResolveAssemblyToPath(name);

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
