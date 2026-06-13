namespace Luxon.Server.Plugin;

public interface ITypedCallInfo<out T> : ICallInfo
{
    T Request { get; }
}
