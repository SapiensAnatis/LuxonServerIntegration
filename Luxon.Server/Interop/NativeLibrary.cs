using System.Reflection;
using System.Runtime.InteropServices;
using Luxon.Server.Interop;

namespace Luxon.Server;

public static class LuxonNativeLibrary
{
    private static string? s_customLibraryPath;

    static LuxonNativeLibrary()
    {
        NativeLibrary.SetDllImportResolver(typeof(LuxonNativeLibrary).Assembly, Resolve);
    }

    public static void SetCustomLibraryPath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        EnsureInitialized();
        s_customLibraryPath = Path.GetFullPath(path);
    }

    public static void ClearCustomLibraryPath()
    {
        EnsureInitialized();
        s_customLibraryPath = null;
    }

    public static void EnsureInitialized()
    {
        _ = typeof(LuxonNativeLibrary);
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, NativeMethods.LibraryName, StringComparison.Ordinal))
        {
            return IntPtr.Zero;
        }

        var customLibraryPath = s_customLibraryPath;
        if (!string.IsNullOrWhiteSpace(customLibraryPath) &&
            NativeLibrary.TryLoad(customLibraryPath, out var handle))
        {
            return handle;
        }

        return IntPtr.Zero;
    }
}
