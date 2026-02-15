using System.Runtime.InteropServices;

namespace Jitzu.Shell.UI.Monitor;

/// <summary>
/// Maps PIDs to their listening TCP ports using GetExtendedTcpTable P/Invoke.
/// </summary>
internal static class PortMapper
{
    public static Dictionary<int, List<int>> GetTcpPortsByPid()
    {
        var result = new Dictionary<int, List<int>>();

        var size = 0;
        // First call to get required buffer size
        var ret = GetExtendedTcpTable(IntPtr.Zero, ref size, false, AF_INET, TCP_TABLE_OWNER_PID_LISTENER, 0);
        if (ret != ERROR_INSUFFICIENT_BUFFER && ret != 0)
            return result;

        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            ret = GetExtendedTcpTable(buffer, ref size, false, AF_INET, TCP_TABLE_OWNER_PID_LISTENER, 0);
            if (ret != 0)
                return result;

            var numEntries = Marshal.ReadInt32(buffer);
            var rowPtr = buffer + 4; // skip dwNumEntries
            var rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();

            for (var i = 0; i < numEntries; i++)
            {
                var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                var port = (int)(((row.dwLocalPort & 0xFF) << 8) | ((row.dwLocalPort >> 8) & 0xFF));
                var pid = unchecked((int)row.dwOwningPid);

                if (!result.TryGetValue(pid, out var ports))
                {
                    ports = [];
                    result[pid] = ports;
                }
                ports.Add(port);

                rowPtr += rowSize;
            }
        }
        catch
        {
            // Silently fail — ports are a nice-to-have
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }

        return result;
    }

    // --- P/Invoke ---

    private const int AF_INET = 2;
    private const int TCP_TABLE_OWNER_PID_LISTENER = 3;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint dwState;
        public uint dwLocalAddr;
        public uint dwLocalPort;
        public uint dwRemoteAddr;
        public uint dwRemotePort;
        public uint dwOwningPid;
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetExtendedTcpTable(
        IntPtr pTcpTable, ref int pdwSize, bool bOrder,
        int ulAf, int tableClass, int reserved);
}
