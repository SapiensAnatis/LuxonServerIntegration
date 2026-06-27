using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal abstract class CallInfo : ICallInfo
{
    private Action? onResultSet;

    public IOperationRequest OperationRequest => throw new NotImplementedException();

    public LuxonGamePluginResult? Result { get; private set; }

    public void Continue()
    {
        this.SetResult(LuxonGamePluginResult.Continue);
    }

    public void Fail(string? msg = null, Dictionary<byte, object>? parameters = null)
    {
        this.SetResult(LuxonGamePluginResult.Fail);
    }

    [MemberNotNullWhen(true, nameof(Result))]
    public bool HasResult()
    {
        return this.Result != null;
    }

    public bool IsProcessed => this.HasResult();

    public void OnResultSet(Action onResultSet)
    {
        this.onResultSet = onResultSet;
    }

    private void SetResult(LuxonGamePluginResult result)
    {
        if (this.Result != null)
        {
            throw new InvalidOperationException("The operation result is already set.");
        }

        this.Result = result;

        this.onResultSet?.Invoke();
    }
}
