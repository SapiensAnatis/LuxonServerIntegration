using Luxon.Ser;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal sealed class RaiseEventRequest : IRaiseEventRequest
{
    public required byte OperationCode { get; set; }

    public required Dictionary<byte, Value> Parameters { get; set; }

    public required byte EvCode { get; set; }
}
