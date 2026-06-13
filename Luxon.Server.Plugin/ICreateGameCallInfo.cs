using LuxonServer.Plugin;

namespace Luxon.Server.Plugin;

public interface ICreateGameCallInfo : ITypedCallInfo<IJoinGameRequest>
{
    // bool IsJoin { get; }
    // bool CreateIfNotExists { get; }
    // Dictionary<string, object>? CreateOptions { get; }
}
