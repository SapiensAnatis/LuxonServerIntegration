using System.Reflection;
using System.Runtime.Loader;

namespace Luxon.Server.Host;

internal sealed class PluginLoadContext(string pluginPath)
    : AssemblyLoadContext(name: $"PluginLoadContext_{pluginPath}", isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName name)
    {
        if (name.Name == "Luxon.Server.Plugin")
        {
            // Load the same Luxon.Server.Plugin assembly as the host app, so that checks like
            // pluginType.IsAssignableTo(typeof(IPluginFactory)) actually work. Returning null forwards
            // to the default resolver.
            return null;
        }

        string? path = _resolver.ResolveAssemblyToPath(name);

        if (path == null)
        {
            return null;
        }

        return LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string name)
    {
        throw new NotSupportedException();
    }
}
