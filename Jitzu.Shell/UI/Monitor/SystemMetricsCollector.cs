using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Jitzu.Shell.UI.Monitor;

/// <summary>
/// Collects system-wide CPU, RAM, disk, and network metrics.
/// CPU is computed from GetSystemTimes deltas; RAM from GlobalMemoryStatusEx;
/// Network from NetworkInterface byte counters.
/// </summary>
internal sealed class SystemMetricsCollector : IDisposable
{
    private const int HistoryCapacity = 120;

    private readonly double[] _cpuHistory = new double[HistoryCapacity];
    private readonly double[] _ramHistory = new double[HistoryCapacity];
    private readonly double[] _netSendHistory = new double[HistoryCapacity];   // bytes/sec
    private readonly double[] _netRecvHistory = new double[HistoryCapacity];   // bytes/sec
    private int _historyCount;
    private int _historyHead; // next write position (ring buffer)

    private long _prevIdle;
    private long _prevKernel;
    private long _prevUser;
    private bool _hasPrevious;

    // Per-core tracking
    private long[]? _prevCoreIdle;
    private long[]? _prevCoreKernel;
    private long[]? _prevCoreUser;

    // Network tracking
    private long _prevNetSent;
    private long _prevNetRecv;
    private DateTime _prevNetTime;
    private bool _hasNetPrevious;

    public ReadOnlySpan<double> CpuHistory => _historyCount == HistoryCapacity
        ? AsOrderedSpan(_cpuHistory)
        : _cpuHistory.AsSpan(0, _historyCount);

    public ReadOnlySpan<double> RamHistory => _historyCount == HistoryCapacity
        ? AsOrderedSpan(_ramHistory)
        : _ramHistory.AsSpan(0, _historyCount);

    public ReadOnlySpan<double> NetSendHistory => _historyCount == HistoryCapacity
        ? AsOrderedSpan(_netSendHistory)
        : _netSendHistory.AsSpan(0, _historyCount);

    public ReadOnlySpan<double> NetRecvHistory => _historyCount == HistoryCapacity
        ? AsOrderedSpan(_netRecvHistory)
        : _netRecvHistory.AsSpan(0, _historyCount);

    public MetricsSnapshot Sample()
    {
        var cpu = SampleCpu();
        var coreCpus = SamplePerCoreCpu();
        var ram = SampleRam();
        var disks = SampleDisks();
        var net = SampleNetwork();

        // Store in ring buffer
        _cpuHistory[_historyHead] = cpu;
        _ramHistory[_historyHead] = ram.UsedPercent;
        _netSendHistory[_historyHead] = net.SendBytesPerSec;
        _netRecvHistory[_historyHead] = net.RecvBytesPerSec;
        _historyHead = (_historyHead + 1) % HistoryCapacity;
        if (_historyCount < HistoryCapacity)
            _historyCount++;

        return new MetricsSnapshot(cpu, coreCpus, ram, disks, net);
    }

    private double SampleCpu()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
            return 0;

        var idleTicks = FileTimeToLong(idle);
        var kernelTicks = FileTimeToLong(kernel);
        var userTicks = FileTimeToLong(user);

        if (!_hasPrevious)
        {
            _prevIdle = idleTicks;
            _prevKernel = kernelTicks;
            _prevUser = userTicks;
            _hasPrevious = true;
            return 0;
        }

        var dIdle = idleTicks - _prevIdle;
        var dKernel = kernelTicks - _prevKernel;
        var dUser = userTicks - _prevUser;

        _prevIdle = idleTicks;
        _prevKernel = kernelTicks;
        _prevUser = userTicks;

        var total = dKernel + dUser; // kernel includes idle time
        if (total == 0) return 0;

        var busy = total - dIdle;
        return Math.Clamp(busy * 100.0 / total, 0, 100);
    }

    private double[] SamplePerCoreCpu()
    {
        var coreCount = Environment.ProcessorCount;
        var structSize = Marshal.SizeOf<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>();
        var bufferSize = structSize * coreCount;
        var buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            var status = NtQuerySystemInformation(
                8 /* SystemProcessorPerformanceInformation */,
                buffer, bufferSize, out var returnLength);

            if (status != 0)
                return [];

            var actualCores = returnLength / structSize;
            var results = new double[actualCores];

            var idle = new long[actualCores];
            var kernel = new long[actualCores];
            var user = new long[actualCores];

            for (var i = 0; i < actualCores; i++)
            {
                var info = Marshal.PtrToStructure<SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION>(buffer + i * structSize);
                idle[i] = info.IdleTime;
                kernel[i] = info.KernelTime;
                user[i] = info.UserTime;
            }

            if (_prevCoreIdle != null && _prevCoreIdle.Length == actualCores)
            {
                for (var i = 0; i < actualCores; i++)
                {
                    var dIdle = idle[i] - _prevCoreIdle[i];
                    var dKernel = kernel[i] - _prevCoreKernel![i];
                    var dUser = user[i] - _prevCoreUser![i];
                    var total = dKernel + dUser;
                    results[i] = total == 0 ? 0 : Math.Clamp((total - dIdle) * 100.0 / total, 0, 100);
                }
            }

            _prevCoreIdle = idle;
            _prevCoreKernel = kernel;
            _prevCoreUser = user;

            return results;
        }
        catch
        {
            return [];
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static RamInfo SampleRam()
    {
        var status = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        if (!GlobalMemoryStatusEx(ref status))
            return new RamInfo(0, 0, 0, 0);

        var totalBytes = (long)status.ullTotalPhys;
        var availBytes = (long)status.ullAvailPhys;
        var usedBytes = totalBytes - availBytes;
        return new RamInfo(status.dwMemoryLoad, totalBytes, usedBytes, availBytes);
    }

    private static DiskInfo[] SampleDisks()
    {
        try
        {
            var drives = DriveInfo.GetDrives();
            var result = new List<DiskInfo>();
            foreach (var d in drives)
            {
                if (!d.IsReady || d.DriveType != DriveType.Fixed)
                    continue;
                result.Add(new DiskInfo(
                    d.Name.TrimEnd('\\'),
                    d.TotalSize,
                    d.TotalSize - d.TotalFreeSpace,
                    d.TotalFreeSpace));
            }
            return result.ToArray();
        }
        catch
        {
            return [];
        }
    }

    private NetInfo SampleNetwork()
    {
        try
        {
            long totalSent = 0, totalRecv = 0;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                    continue;
                if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                    continue;
                var stats = nic.GetIPv4Statistics();
                totalSent += stats.BytesSent;
                totalRecv += stats.BytesReceived;
            }

            var now = DateTime.UtcNow;
            if (!_hasNetPrevious)
            {
                _prevNetSent = totalSent;
                _prevNetRecv = totalRecv;
                _prevNetTime = now;
                _hasNetPrevious = true;
                return new NetInfo(0, 0);
            }

            var elapsed = (now - _prevNetTime).TotalSeconds;
            if (elapsed <= 0) return new NetInfo(0, 0);

            var sendPerSec = (totalSent - _prevNetSent) / elapsed;
            var recvPerSec = (totalRecv - _prevNetRecv) / elapsed;

            _prevNetSent = totalSent;
            _prevNetRecv = totalRecv;
            _prevNetTime = now;

            return new NetInfo(Math.Max(0, sendPerSec), Math.Max(0, recvPerSec));
        }
        catch
        {
            return new NetInfo(0, 0);
        }
    }

    /// <summary>
    /// Returns the ring buffer contents in chronological order when the buffer is full.
    /// </summary>
    private ReadOnlySpan<double> AsOrderedSpan(double[] ring)
    {
        // When full, _historyHead points to the oldest entry
        var ordered = new double[HistoryCapacity];
        var tail = _historyHead; // oldest
        for (var i = 0; i < HistoryCapacity; i++)
            ordered[i] = ring[(tail + i) % HistoryCapacity];
        return ordered;
    }

    public void Dispose() { }

    // --- P/Invoke ---

    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEM_PROCESSOR_PERFORMANCE_INFORMATION
    {
        public long IdleTime;
        public long KernelTime;
        public long UserTime;
        public long DpcTime;
        public long InterruptTime;
        public int InterruptCount;
    }

    [DllImport("ntdll.dll")]
    private static extern int NtQuerySystemInformation(
        int systemInformationClass, IntPtr systemInformation,
        int systemInformationLength, out int returnLength);

    private static long FileTimeToLong(FILETIME ft) =>
        ((long)ft.dwHighDateTime << 32) | ft.dwLowDateTime;
}

internal record MetricsSnapshot(double CpuPercent, double[] CoreCpuPercents, RamInfo Ram, DiskInfo[] Disks, NetInfo Net);
internal record RamInfo(double UsedPercent, long TotalBytes, long UsedBytes, long AvailableBytes);
internal record DiskInfo(string Name, long TotalBytes, long UsedBytes, long FreeBytes);
internal record NetInfo(double SendBytesPerSec, double RecvBytesPerSec);
