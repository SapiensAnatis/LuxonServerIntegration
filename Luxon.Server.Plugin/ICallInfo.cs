using System.Collections.Generic;
using Luxon.Ser;

namespace Luxon.Server.Plugin;

public interface ICallInfo
{
    IOperationRequest OperationRequest { get; }

    void Continue();
    void Fail(string? msg = null, Dictionary<byte, object>? parameters = null);
    void StrictModeCheck(out string errorMessage);
}
