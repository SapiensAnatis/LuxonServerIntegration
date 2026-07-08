using Luxon.Server.Interop;

namespace Luxon.Server;

public sealed class ByteArrayBuffer : NativeResource
{
    internal ByteArrayBuffer(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public static ByteArrayBuffer Create()
        => new(LuxonInteropHelpers.Checked(NativeMethods.createByteArray), true);

    public static ByteArrayBuffer FromBytes(ReadOnlySpan<byte> data)
    {
        var buffer = Create();
        try
        {
            buffer.Replace(data);
            return buffer;
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    public nuint Size => LuxonInteropHelpers.Checked(() => NativeMethods.getByteArraySize(Handle));

    public int Length => checked((int)Size);

    public byte[] Bytes => ToArray();

    public IntPtr DataPointer => LuxonInteropHelpers.Checked(() => NativeMethods.getByteArrayDataPtr(Handle));

    public nuint CopyTo(byte[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return LuxonInteropHelpers.Checked(() => NativeMethods.copyFromByteArray(Handle, destination, (nuint)destination.Length));
    }

    public byte[] ToArray()
    {
        var size = Size;
        if (size == 0)
        {
            return Array.Empty<byte>();
        }

        var buffer = new byte[checked((int)size)];
        var copied = CopyTo(buffer);
        var actual = copied > (nuint)buffer.Length ? buffer.Length : (int)copied;

        if (actual != buffer.Length)
        {
            Array.Resize(ref buffer, actual);
        }

        return buffer;
    }

    public void Append(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
        {
            return;
        }

        var bytes = data.ToArray();
        LuxonInteropHelpers.Checked(() => NativeMethods.appendToByteArray(Handle, bytes, (nuint)bytes.Length));
    }

    public void Replace(ReadOnlySpan<byte> data)
    {
        var bytes = data.ToArray();
        LuxonInteropHelpers.Checked(() => NativeMethods.copyToByteArray(Handle, bytes, (nuint)bytes.Length));
    }

    public void Push(byte value)
        => LuxonInteropHelpers.Checked(() => NativeMethods.pushToByteArray(Handle, value));

    public byte this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            return LuxonInteropHelpers.Checked(() => NativeMethods.getInByteArray(Handle, (nuint)index));
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            LuxonInteropHelpers.Checked(() => NativeMethods.setInByteArray(Handle, (nuint)index, value));
        }
    }

    protected override void ReleaseHandle(IntPtr handle)
        => NativeMethods.destroyByteArray(handle);
}

public sealed class SerValueRef : NativeResource
{
    internal SerValueRef(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public global::Luxon.Ser.Value ManagedValue
    {
        get => ToManagedValue();
        set => Replace(value);
    }

    public static SerValueRef Create()
        => new(LuxonInteropHelpers.Checked(NativeMethods.createSerValue), true);

    internal static SerValueRef CreatePopulated(Action<IntPtr> populator)
    {
        ArgumentNullException.ThrowIfNull(populator);

        var result = Create();
        try
        {
            populator(result.DangerousHandle);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static SerValueRef FromBytes(ReadOnlySpan<byte> data)
    {
        var result = Create();
        try
        {
            result.Deserialize(data);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static SerValueRef FromByteArray(ByteArrayBuffer data)
    {
        LuxonInteropHelpers.ThrowIfDisposed(data, nameof(data));

        var result = Create();
        try
        {
            result.Deserialize(data);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static SerValueRef FromManaged(global::Luxon.Ser.Value value)
    {
        var result = Create();
        try
        {
            result.Replace(value);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public ByteArrayBuffer SerializeToByteArray()
        => new(LuxonInteropHelpers.Checked(() => NativeMethods.serializeSerValue(Handle)), true);

    public byte[] ToBytes()
    {
        using var buffer = SerializeToByteArray();
        return buffer.ToArray();
    }

    public bool TryDeserialize(ReadOnlySpan<byte> data)
    {
        var bytes = data.ToArray();
        return LuxonInteropHelpers.Checked(() => NativeMethods.deserializeSerValue(bytes, (nuint)bytes.Length, Handle));
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        if (!TryDeserialize(data))
        {
            throw new InvalidDataException("Native ser value deserialization returned false.");
        }
    }

    public bool TryDeserialize(ByteArrayBuffer data)
    {
        LuxonInteropHelpers.ThrowIfDisposed(data, nameof(data));
        return LuxonInteropHelpers.Checked(() => NativeMethods.deserializeSerValueFromByteArray(data.DangerousHandle, Handle));
    }

    public void Deserialize(ByteArrayBuffer data)
    {
        if (!TryDeserialize(data))
        {
            throw new InvalidDataException("Native ser value deserialization from byte array returned false.");
        }
    }

    public global::Luxon.Ser.Value ToManagedValue()
        => LuxonSerBridge.DeserializeValue(ToBytes());

    public void Replace(global::Luxon.Ser.Value value)
        => Deserialize(LuxonSerBridge.SerializeValue(value));

    public SerValueRef Clone()
        => FromBytes(ToBytes());

    protected override void ReleaseHandle(IntPtr handle)
        => NativeMethods.destroySerValue(handle);
}

public sealed class SerMessageRef : NativeResource
{
    internal SerMessageRef(IntPtr handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public global::Luxon.Ser.Message ManagedMessage
    {
        get => ToManagedMessage();
        set => Replace(value);
    }

    public static SerMessageRef Create()
        => new(LuxonInteropHelpers.Checked(NativeMethods.createSerMessage), true);

    internal static SerMessageRef CreatePopulated(Action<IntPtr> populator)
    {
        ArgumentNullException.ThrowIfNull(populator);

        var result = Create();
        try
        {
            populator(result.DangerousHandle);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static SerMessageRef FromBytes(ReadOnlySpan<byte> data)
    {
        var result = Create();
        try
        {
            result.Deserialize(data);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static SerMessageRef FromByteArray(ByteArrayBuffer data)
    {
        LuxonInteropHelpers.ThrowIfDisposed(data, nameof(data));

        var result = Create();
        try
        {
            result.Deserialize(data);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public static SerMessageRef FromManaged(global::Luxon.Ser.Message message)
    {
        var result = Create();
        try
        {
            result.Replace(message);
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }

    public ByteArrayBuffer SerializeToByteArray()
        => new(LuxonInteropHelpers.Checked(() => NativeMethods.serializeSerMessage(Handle)), true);

    public byte[] ToBytes()
    {
        using var buffer = SerializeToByteArray();
        return buffer.ToArray();
    }

    public bool TryDeserialize(ReadOnlySpan<byte> data)
    {
        var bytes = data.ToArray();
        return LuxonInteropHelpers.Checked(() => NativeMethods.deserializeSerMessage(bytes, (nuint)bytes.Length, Handle));
    }

    public void Deserialize(ReadOnlySpan<byte> data)
    {
        if (!TryDeserialize(data))
        {
            throw new InvalidDataException("Native ser message deserialization returned false.");
        }
    }

    public bool TryDeserialize(ByteArrayBuffer data)
    {
        LuxonInteropHelpers.ThrowIfDisposed(data, nameof(data));
        return LuxonInteropHelpers.Checked(() => NativeMethods.deserializeSerMessageFromByteArray(data.DangerousHandle, Handle));
    }

    public void Deserialize(ByteArrayBuffer data)
    {
        if (!TryDeserialize(data))
        {
            throw new InvalidDataException("Native ser message deserialization from byte array returned false.");
        }
    }

    public global::Luxon.Ser.Message ToManagedMessage()
        => LuxonSerBridge.DeserializeMessage(ToBytes());

    public void Replace(global::Luxon.Ser.Message message)
        => Deserialize(LuxonSerBridge.SerializeMessage(message));

    public SerMessageRef Clone()
        => FromBytes(ToBytes());

    protected override void ReleaseHandle(IntPtr handle)
        => NativeMethods.destroySerMessage(handle);
}
