namespace Luxon.Server.Plugin;

public readonly record struct SendParameters
{
    public byte ChannelId { get; init; }

    public LuxonDeliveryMode DeliveryMode { get; init; }

    public bool Encrypted { get; init; }
}
