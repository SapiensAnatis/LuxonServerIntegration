using Luxon.Ser;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal class CallInfo : ICallInfo
{
    public IOperationRequest OperationRequest => throw new NotImplementedException();

    public LuxonGamePluginResult? Result { get; private set; }

    public void Continue()
    {
        if (Result != null)
        {
            throw new InvalidOperationException("The operation result is already set.");
        }

        Result = LuxonGamePluginResult.Continue;
    }

    public void Fail(string? msg = null, Dictionary<byte, object>? parameters = null)
    {
        if (Result != null)
        {
            throw new InvalidOperationException("The operation result is already set.");
        }

        Result = LuxonGamePluginResult.Fail;
    }
}
