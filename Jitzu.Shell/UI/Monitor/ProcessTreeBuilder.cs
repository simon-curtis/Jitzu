using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Jitzu.Shell.UI.Monitor;

/// <summary>
/// Builds a parent-child process tree using CreateToolhelp32Snapshot P/Invoke,
/// combined with Process.GetProcesses() for memory and CPU stats.
/// </summary>
internal sealed class ProcessTreeBuilder
{
    private Dictionary<int, TimeSpan> _prevCpuTimes = new();
    private DateTime _prevSampleTime = DateTime.UtcNow;

    public List<ProcessRow> BuildTree(bool shellChildrenOnly, int shellPid)
    {
        var snapshot = TakeSnapshot();
        var processes = GetProcessInfo();
        var portsByPid = PortMapper.GetTcpPortsByPid();
        var now = DateTime.UtcNow;
        var elapsed = (now - _prevSampleTime).TotalSeconds;
        if (elapsed < 0.1) elapsed = 1.0;

        // Build parent→children map
        var children = new Dictionary<int, List<int>>();
        var parentOf = new Dictionary<int, int>();
        foreach (var (pid, ppid) in snapshot)
        {
            parentOf[pid] = ppid;
            if (!children.ContainsKey(ppid))
                children[ppid] = [];
            children[ppid].Add(pid);
        }

        // Compute CPU%
        var newCpuTimes = new Dictionary<int, TimeSpan>();
        var cpuPercents = new Dictionary<int, double>();
        var processorCount = Environment.ProcessorCount;

        foreach (var (pid, info) in processes)
        {
            newCpuTimes[pid] = info.CpuTime;
            if (_prevCpuTimes.TryGetValue(pid, out var prev))
            {
                var delta = (info.CpuTime - prev).TotalSeconds;
                cpuPercents[pid] = Math.Clamp(delta / elapsed / processorCount * 100.0, 0, 100);
            }
        }

        _prevCpuTimes = newCpuTimes;
        _prevSampleTime = now;

        var result = new List<ProcessRow>();

        var visited = new HashSet<int>();

        if (shellChildrenOnly)
        {
            // Walk descendants of shellPid
            if (children.ContainsKey(shellPid))
            {
                foreach (var childPid in children[shellPid])
                    WalkTree(childPid, 0, "", true, children, processes, cpuPercents, portsByPid, result, visited);
            }
        }
        else
        {
            // Find root processes (those whose parent isn't in the snapshot)
            var allPids = new HashSet<int>(snapshot.Keys);
            var roots = new List<int>();
            foreach (var (pid, ppid) in snapshot)
            {
                if (!allPids.Contains(ppid) || ppid == 0)
                    roots.Add(pid);
            }
            roots.Sort();

            foreach (var root in roots)
                WalkTree(root, 0, "", root == roots[^1], children, processes, cpuPercents, portsByPid, result, visited);
        }

        return result;
    }

    private static void WalkTree(
        int pid, int depth, string prefix, bool isLast,
        Dictionary<int, List<int>> children,
        Dictionary<int, ProcessInfo> processes,
        Dictionary<int, double> cpuPercents,
        Dictionary<int, List<int>> portsByPid,
        List<ProcessRow> result,
        HashSet<int> visited)
    {
        if (!visited.Add(pid))
            return; // cycle detected — skip

        var treePrefix = depth == 0 ? "" : prefix + (isLast ? "\u2514\u2500" : "\u251c\u2500");
        var info = processes.GetValueOrDefault(pid);
        var cpu = cpuPercents.GetValueOrDefault(pid);
        var ports = portsByPid.TryGetValue(pid, out var p) ? p.ToArray() : [];

        result.Add(new ProcessRow(
            pid,
            info?.Name ?? "?",
            info?.MemoryBytes ?? 0,
            cpu,
            depth,
            treePrefix,
            ports));

        if (!children.TryGetValue(pid, out var kids))
            return;

        kids.Sort();
        var childPrefix = depth == 0 ? "" : prefix + (isLast ? "  " : "\u2502 ");
        for (var i = 0; i < kids.Count; i++)
            WalkTree(kids[i], depth + 1, childPrefix, i == kids.Count - 1, children, processes, cpuPercents, portsByPid, result, visited);
    }

    private static Dictionary<int, ProcessInfo> GetProcessInfo()
    {
        var result = new Dictionary<int, ProcessInfo>();
        try
        {
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    result[p.Id] = new ProcessInfo(p.ProcessName, p.WorkingSet64, p.TotalProcessorTime);
                }
                catch
                {
                    result[p.Id] = new ProcessInfo(p.ProcessName, 0, TimeSpan.Zero);
                }
                finally
                {
                    p.Dispose();
                }
            }
        }
        catch { }
        return result;
    }

    /// <summary>
    /// Uses CreateToolhelp32Snapshot to get PID → ParentPID mapping.
    /// </summary>
    private static Dictionary<int, int> TakeSnapshot()
    {
        var map = new Dictionary<int, int>();
        var handle = CreateToolhelp32Snapshot(0x00000002 /* TH32CS_SNAPPROCESS */, 0);
        if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            return map;

        try
        {
            var entry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>() };
            if (!Process32First(handle, ref entry))
                return map;

            do
            {
                map[(int)entry.th32ProcessID] = (int)entry.th32ParentProcessID;
            } while (Process32Next(handle, ref entry));
        }
        finally
        {
            CloseHandle(handle);
        }

        return map;
    }

    // --- P/Invoke ---

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct PROCESSENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID;
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExeFile;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);
}

internal record ProcessRow(int Pid, string Name, long MemoryBytes, double CpuPercent, int IndentLevel, string TreePrefix, int[] Ports);
internal record ProcessInfo(string Name, long MemoryBytes, TimeSpan CpuTime);
