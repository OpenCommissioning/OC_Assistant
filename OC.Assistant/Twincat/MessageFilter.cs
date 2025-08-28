using System.Runtime.InteropServices;

namespace OC.Assistant.Twincat;

[ComImport, Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] 
public interface IOleMessageFilter 
{ 
    [PreserveSig] 
    int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo); 


    [PreserveSig] 
    int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType); 


    [PreserveSig] 
    int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType); 
} 

public class MessageFilter : IOleMessageFilter
{
    public static void Register()
    {
        IOleMessageFilter newFilter = new MessageFilter();
        var result = CoRegisterMessageFilter(newFilter, out _);

        if (result != 0)
        {
            Sdk.Logger.LogError(typeof(MessageFilter), $"CoRegisterMessageFilter failed with error {result}");
        }
    }

    public static void Revoke()
    {
        _ = CoRegisterMessageFilter(null, out _);
    }

    int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
    {
        // return flag SERVERCALL_ISHANDLED
        return 0;
    }

    int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
    {
        // Thread call was refused, try again
        if (dwRejectType == 2) // flag SERVERCALL_RETRYLATER 
        {
            // retry thread call at once, if return value >= 0 & < 100
            return 100;
        }

        return -1;
    }
    
    int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
    {
        // return flag PENDINGMSG_WAITDEFPROCESS 
        return 2;
    }
    
    [DllImport("Ole32.dll")]
    private static extern int CoRegisterMessageFilter(IOleMessageFilter? newFilter, out IOleMessageFilter oldFilter);
}