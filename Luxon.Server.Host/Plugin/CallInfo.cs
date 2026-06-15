using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Luxon.Ser;
using Luxon.Server.Plugin;

namespace Luxon.Server.Host.Plugin;

internal abstract class CallInfo : ICallInfo, IDisposable
{
    private bool _disposed;
    private readonly ManualResetEventSlim _resultSetEvent = new();

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

    [MemberNotNull(nameof(Result))]
    public void WaitForResult()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Result == null && !_resultSetEvent.Wait(TimeSpan.FromSeconds(3)))
        {
            throw new TimeoutException("Plugin did not set result in time");
        }

        if (Result == null)
        {
            throw new UnreachableException("Result not set but _resultSetEvent was set");
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _resultSetEvent.Dispose();
    }

    private void SetResult(LuxonGamePluginResult result)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Result != null)
        {
            throw new InvalidOperationException("The operation result is already set.");
        }

        Result = result;
        _resultSetEvent.Set();
    }
}
