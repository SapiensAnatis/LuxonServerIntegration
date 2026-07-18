using System.Runtime.InteropServices;
using System.Text;
using Luxon.Server.Interop;

namespace Luxon.Server;

public enum LuxonServerType
{
    None = 0,
    NameServer = 1,
    MasterServer = 2,
    GameServer = 3,
}

public enum LuxonServerProtocol
{
    Udp = 0,
    Tcp = 1,
    WebSocket = 2,
}

public enum LuxonDeliveryMode
{
    Unreliable = 0,
    Reliable = 1,
    Unsequenced = 2,
}

public enum LuxonEventReceiversType
{
    All = 0,
    Group = 1,
    Actors = 2,
}

public enum LuxonGamePluginResult : byte
{
    Continue = 0,
    Fail = 1,
    Cancel = 2,
    Async = 3,
}

public enum LuxonLogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warn = 3,
    Error = 4,
    Critical = 5,
    Off = 6,
}

public readonly record struct EventRoutingMetadata(
    byte Code,
    int SenderActorId,
    LuxonDeliveryMode DeliveryMode,
    byte Channel,
    byte InterestGroup
);

public readonly record struct GameConfigState(
    byte Flags,
    bool IsCreated,
    bool IsOpen,
    bool IsVisible,
    byte MaxPeers,
    int MasterActor
);

public readonly record struct GameTtlConfig(int PlayerTtl, int EmptyTtl);

public readonly record struct JoinValidationResult(
    bool Allowed,
    short ErrorCode,
    string ErrorMessage
);

public readonly record struct LobbyQueryResult(bool Success, string Payload);

public readonly record struct CreateGameResult(Game? Game, SerMessageRef ErrorResponse);

public sealed class LuxonException : Exception
{
    public LuxonException(string errorType, string message)
        : base(message)
    {
        ErrorType = string.IsNullOrWhiteSpace(errorType) ? "LuxonError" : errorType;
    }

    public string ErrorType { get; }

    public override string ToString() => $"{ErrorType}: {Message}";
}

public sealed class LuxonFeatureUnavailableException : NotSupportedException
{
    public LuxonFeatureUnavailableException(string featureName, Exception? innerException = null)
        : base(
            $"The loaded Luxon Server FFI library does not expose the '{featureName}' feature.",
            innerException
        )
    {
        FeatureName = featureName;
    }

    public string FeatureName { get; }
}

public abstract class NativeResource : IDisposable
{
    private IntPtr _handle;
    private readonly bool _ownsHandle;
    private int _disposed;

    protected NativeResource(IntPtr handle, bool ownsHandle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Native handle cannot be zero.", nameof(handle));
        }

        _handle = handle;
        _ownsHandle = ownsHandle;

        if (!ownsHandle)
        {
            GC.SuppressFinalize(this);
        }
    }

    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    public IntPtr DangerousHandle => _handle;

    protected IntPtr Handle
    {
        get
        {
            ThrowIfDisposed();
            return _handle;
        }
    }

    protected void ThrowIfDisposed()
    {
        if (IsDisposed || _handle == IntPtr.Zero)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    protected abstract void ReleaseHandle(IntPtr handle);

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);

        if (_ownsHandle && handle != IntPtr.Zero)
        {
            try
            {
                ReleaseHandle(handle);

                try
                {
                    if (NativeMethods.luxonHasError())
                    {
                        NativeMethods.luxonClearLastError();
                    }
                }
                catch { }
            }
            catch { }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NativeResource()
    {
        Dispose(false);
    }

    internal IntPtr DetachHandle()
    {
        ThrowIfDisposed();

        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        Interlocked.Exchange(ref _disposed, 1);
        GC.SuppressFinalize(this);
        return handle;
    }
}

internal static class LuxonInteropHelpers
{
    static LuxonInteropHelpers()
    {
        LuxonNativeLibrary.EnsureInitialized();
    }

    internal static T Checked<T>(Func<T> action)
    {
        var result = action();
        ThrowIfPendingError();
        return result;
    }

    internal static void Checked(Action action)
    {
        action();
        ThrowIfPendingError();
    }

    internal static void ThrowIfPendingError()
    {
        if (!NativeMethods.luxonHasError())
        {
            return;
        }

        var errorType = PtrToUtf8String(NativeMethods.luxonGetLastErrorType());
        var message = PtrToUtf8String(NativeMethods.luxonGetLastErrorMessage());

        NativeMethods.luxonClearLastError();
        throw new LuxonException(errorType, message);
    }

    internal static string PtrToUtf8String(IntPtr ptr) =>
        ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(ptr) ?? string.Empty;

    internal static string DecodeUtf8(byte[] buffer, nuint? explicitLength = null)
    {
        if (buffer.Length == 0)
        {
            return string.Empty;
        }

        int length;
        if (explicitLength.HasValue)
        {
            var written = explicitLength.Value;
            length = written > (nuint)buffer.Length ? buffer.Length : (int)written;

            while (length > 0 && buffer[length - 1] == 0)
            {
                length--;
            }
        }
        else
        {
            length = Array.IndexOf(buffer, (byte)0);
            if (length < 0)
            {
                length = buffer.Length;
            }
        }

        return length == 0 ? string.Empty : Encoding.UTF8.GetString(buffer, 0, length);
    }

    internal static void WriteUtf8CString(string? value, IntPtr destination, nuint maxLength)
    {
        if (destination == IntPtr.Zero || maxLength == 0)
        {
            return;
        }

        var text = value ?? string.Empty;
        var bytes = Encoding.UTF8.GetBytes(text);
        var max = maxLength > (nuint)int.MaxValue ? int.MaxValue : (int)maxLength;

        if (max <= 0)
        {
            return;
        }

        var copyLength = Math.Min(bytes.Length, max - 1);

        if (copyLength > 0)
        {
            Marshal.Copy(bytes, 0, destination, copyLength);
        }

        Marshal.WriteByte(destination, copyLength, 0);
    }

    internal static bool ByteToBool(byte value) => value != 0;

    internal static byte BoolToByte(bool value) => value ? (byte)1 : (byte)0;

    internal static void WriteBoolByte(IntPtr destination, bool value)
    {
        if (destination != IntPtr.Zero)
        {
            Marshal.WriteByte(destination, value ? (byte)1 : (byte)0);
        }
    }

    internal static void ThrowIfDisposed(NativeResource resource, string paramName)
    {
        ArgumentNullException.ThrowIfNull(resource, paramName);

        if (resource.IsDisposed)
        {
            throw new ObjectDisposedException(resource.GetType().Name);
        }
    }
}
