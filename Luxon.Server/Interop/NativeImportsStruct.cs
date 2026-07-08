using System.Runtime.InteropServices;

namespace Luxon.Server.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeImportsStruct
{
    public IntPtr gamePluginOnAttach;
    public IntPtr gamePluginOnCreateGame;
    public IntPtr gamePluginBeforeJoin;
    public IntPtr gamePluginOnJoinGame;
    public IntPtr gamePluginOnLeave;
    public IntPtr gamePluginOnRaiseEvent;
    public IntPtr gamePluginBeforeSetProperties;
    public IntPtr gamePluginOnSetProperties;
    public IntPtr gamePluginBeforeCloseGame;
    public IntPtr gamePluginOnCloseGame;
    public IntPtr authPluginOnAuthenticate;
    public IntPtr customLogSink;
    public IntPtr hookpointMasterServerHandleOperationRequestJoinGame;
    public IntPtr hookpointMasterServerHandleOperationRequestCreateGame;
    public IntPtr hookpointHandlerBaseHandleENetCommandOnMessage;
    public IntPtr hookpointAppLoadAppSettings;
}
