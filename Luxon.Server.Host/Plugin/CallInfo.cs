using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal abstract class CallInfo : ICallInfo
{
    private List<Action<LuxonGamePluginResult>> onResultSetCallbacks = [];

    public required IOperationRequest OperationRequest { get; init; }

    public LuxonGamePluginResult? Result { get; private set; }

    public void Continue()
    {
        this.SetResult(LuxonGamePluginResult.Continue);
    }

    public void Fail(string? msg = null, Dictionary<byte, object>? parameters = null)
    {
        this.SetResult(LuxonGamePluginResult.Fail);
    }

    public void Cancel()
    {
        this.SetResult(LuxonGamePluginResult.Cancel);
    }

    [MemberNotNullWhen(true, nameof(Result))]
    public bool HasResult()
    {
        return this.Result != null;
    }

    public bool IsProcessed => this.HasResult();

    /// <summary>
    /// Configures a callback to be invoked when a result is set. Can be called multiple times to set up multiple callbacks.
    /// </summary>
    /// <param name="onResultSet">The callback to invoke. Accepts a single parameter that is the type of result that was set.</param>
    public void OnResultSet(Action<LuxonGamePluginResult> onResultSet)
    {
        this.onResultSetCallbacks.Add(onResultSet);
    }

    private void SetResult(LuxonGamePluginResult result)
    {
        if (this.Result != null)
        {
            throw new InvalidOperationException("The operation result is already set.");
        }

        this.Result = result;

        foreach (Action<LuxonGamePluginResult> cb in this.onResultSetCallbacks)
        {
            cb.Invoke(result);
        }
    }
}
