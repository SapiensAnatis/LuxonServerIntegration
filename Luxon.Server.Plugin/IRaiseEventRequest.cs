using Luxon.Ser;

namespace Luxon.Server.Plugin;

public interface IRaiseEventRequest : IOperationRequest
{
    byte EvCode { get; }
}
