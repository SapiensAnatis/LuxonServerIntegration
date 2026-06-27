using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;

namespace Luxon.Server.Plugin;

public interface ICallInfo
{
    [Experimental("LUX0001", Message = "Not yet implemented")]
    IOperationRequest OperationRequest { get; }

    void Continue();

    bool IsProcessed { get; }

    void Fail(string? msg = null, Dictionary<byte, object>? parameters = null);
}
