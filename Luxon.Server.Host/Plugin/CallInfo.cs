using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal abstract class CallInfo : ICallInfo
{
    private Action? _onResultSet;

    public IOperationRequest OperationRequest => throw new NotImplementedException();

    public LuxonGamePluginResult? Result { get; private set; }

    public void Continue()
    {
        SetResult(LuxonGamePluginResult.Continue);
    }

    public void Fail(string? msg = null, Dictionary<byte, object>? parameters = null)
    {
        SetResult(LuxonGamePluginResult.Fail);
    }

    [MemberNotNullWhen(true, nameof(Result))]
    public bool HasResult()
    {
        return Result != null;
    }

    public void OnResultSet(Action onResultSet)
    {
        _onResultSet = onResultSet;
    }

    private void SetResult(LuxonGamePluginResult result)
    {
        if (Result != null)
        {
            throw new InvalidOperationException("The operation result is already set.");
        }

        Result = result;

        _onResultSet?.Invoke();
    }
}
