using System.Text;

namespace Luxon.Server;

public static class LuxonSerBridge
{
    public static byte[] SerializeMessage(global::Luxon.Ser.Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var protocol = new global::Luxon.Ser.IPCBinaryProtocol();
        return protocol.Serialize(message);
    }

    public static global::Luxon.Ser.Message DeserializeMessage(ReadOnlySpan<byte> bytes)
    {
        var protocol = new global::Luxon.Ser.IPCBinaryProtocol();
        return protocol.Deserialize(bytes);
    }

    public static byte[] SerializeValue(global::Luxon.Ser.Value value)
    {
        var protocol = new global::Luxon.Ser.IPCBinaryProtocol();

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

        protocol.EncodeValue(writer, value, 0);
        writer.Flush();

        return ms.ToArray();
    }

    public static global::Luxon.Ser.Value DeserializeValue(ReadOnlySpan<byte> bytes)
    {
        var protocol = new global::Luxon.Ser.IPCBinaryProtocol();

        using var ms = new MemoryStream(bytes.ToArray());
        using var reader = new BinaryReader(ms, Encoding.UTF8, leaveOpen: false);

        return protocol.DecodeValue(reader, 0);
    }
}
