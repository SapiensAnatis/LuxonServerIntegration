using Luxon.Server.Interop;
using Microsoft.Win32.SafeHandles;

namespace Luxon.Server;

public sealed class CoroutineHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public CoroutineHandle()
        : base(true) { }

    public void Resume(LuxonGamePluginResult result)
    {
        LuxonInteropHelpers.Checked(() => NativeMethods.luxonResumeCoroutine(this, (byte)result));
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.luxonDestroyCoroutine(this);
        return true;
    }
}
