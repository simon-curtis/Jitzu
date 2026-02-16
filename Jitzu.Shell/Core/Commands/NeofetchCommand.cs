using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays system information with OS-specific ASCII art, styled after neofetch.
/// </summary>
public class NeofetchCommand : CommandBase
{
    private const int ArtWidth = 42;
    private const int ArtPadding = 2;

    public NeofetchCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var art = GetAsciiArt();
        var info = BuildInfoLines();
        var output = MergeArtAndInfo(art, info);
        return Task.FromResult(new ShellResult(ResultType.Jitzu, output, null));
    }

    private static string MergeArtAndInfo(string[] art, List<string> info)
    {
        var sb = new StringBuilder();
        var maxLines = Math.Max(art.Length, info.Count);

        for (var i = 0; i < maxLines; i++)
        {
            var artLine = i < art.Length ? art[i] : "";
            var infoLine = i < info.Count ? info[i] : "";

            // Pad art to consistent visual width (strip ANSI for length calc)
            var visibleLength = StripAnsi(artLine).Length;
            var padding = Math.Max(0, ArtWidth + ArtPadding - visibleLength);

            sb.Append(artLine);
            sb.Append(new string(' ', padding));
            sb.AppendLine(infoLine);
        }

        return sb.ToString().TrimEnd();
    }

    private static string StripAnsi(string text)
    {
        var sb = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            if (text[i] == '\e' && i + 1 < text.Length && text[i + 1] == '[')
            {
                // Skip to end of ANSI sequence (letter terminator)
                i += 2;
                while (i < text.Length && text[i] is (>= '0' and <= '9') or ';')
                    i++;
                if (i < text.Length) i++; // skip the terminator character
            }
            else
            {
                sb.Append(text[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    private List<string> BuildInfoLines()
    {
        var reset = ThemeConfig.Reset;
        var bold = ThemeConfig.Bold;
        var (labelColor, titleColor) = GetColorScheme();

        var user = Environment.UserName;
        var host = Environment.MachineName;
        var title = $"{bold}{titleColor}{user}@{host}{reset}";
        var separator = $"{titleColor}{new string('-', user.Length + 1 + host.Length)}{reset}";

        var lines = new List<string>(16)
        {
            title,
            separator,
            InfoLine(labelColor, "OS", GetOsName()),
            InfoLine(labelColor, "Host", GetHostName()),
            InfoLine(labelColor, "Kernel", GetKernelVersion()),
            InfoLine(labelColor, "Uptime", GetUptime()),
            InfoLine(labelColor, "Shell", "Jitzu Shell"),
            InfoLine(labelColor, "Terminal", GetTerminal()),
            InfoLine(labelColor, "CPU", GetCpuName()),
            InfoLine(labelColor, "Memory", GetMemoryInfo()),
            InfoLine(labelColor, "Disk (/)", GetDiskInfo()),
            "",
            BuildColorRow(normal: true),
            BuildColorRow(normal: false)
        };

        return lines;
    }

    private static string InfoLine(string labelColor, string label, string value)
        => $"{labelColor}{ThemeConfig.Bold}{label}{ThemeConfig.Reset}: {value}";

    // --- OS Detection & Art ---

    private static (string LabelColor, string TitleColor) GetColorScheme()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return ("\e[38;5;39m", "\e[38;5;39m");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return ("\e[38;5;76m", "\e[38;5;76m");
        // Linux
        return ("\e[38;5;178m", "\e[38;5;178m");
    }

    private static string[] GetAsciiArt()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetWindowsArt();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return GetMacOsArt();
        return GetLinuxArt();
    }

    private static string[] GetWindowsArt()
    {
        var b = "\e[38;5;39m";  // bright blue
        var r = ThemeConfig.Reset;
        return
        [
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}                                          {r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
            $"{b}████████████████████  ████████████████████{r}",
        ];
    }

    private static string[] GetLinuxArt()
    {
        var y = "\e[38;5;178m"; // yellow
        var w = "\e[38;5;255m"; // white
        var k = "\e[38;5;240m"; // dark gray
        var r = ThemeConfig.Reset;
        return
        [
            $"{k}            .---.{r}",
            $"{k}           /     \\{r}",
            $"{k}           \\.@-@./{r}",
            $"{k}           /`\\_/`\\{r}",
            $"{k}          //  _  \\\\{r}",
            $"{y}         | \\     )|_{r}",
            $"{y}        /`  \\   /`  \\{r}",
            $"{w}       /    |   |    \\{r}",
            $"{w}      /   __|   |__   \\{r}",
            $"{w}     |   /  |   |  \\   |{r}",
            $"{w}     |  |   |   |   |  |{r}",
            $"{y}     |  |   \\___/   |  |{r}",
            $"{y}     |  |    ___    |  |{r}",
            $"{y}     |  |   /   \\   |  |{r}",
            $"{y}      \\_/  /     \\  \\_/{r}",
            $"{y}          /       \\{r}",
            $"{y}          \\_______/{r}",
        ];
    }

    private static string[] GetMacOsArt()
    {
        var g  = "\e[38;5;76m";  // green
        var y  = "\e[38;5;178m"; // yellow
        var o  = "\e[38;5;208m"; // orange
        var rd = "\e[38;5;196m"; // red
        var p  = "\e[38;5;129m"; // purple
        var b  = "\e[38;5;39m";  // blue
        var r  = ThemeConfig.Reset;
        return
        [
            $"{g}                  'c.{r}",
            $"{g}                 ,xNMM.{r}",
            $"{g}               .OMMMMo{r}",
            $"{g}               OMMM0,{r}",
            $"{g}     .;loddo:' loolloddol;.{r}",
            $"{y}   cKMMMMMMMMMMNWMMMMMMMMMM0:{r}",
            $"{y} .KMMMMMMMMMMMMMMMMMMMMMMMWd.{r}",
            $"{o} XMMMMMMMMMMMMMMMMMMMMMMMX.{r}",
            $"{o};MMMMMMMMMMMMMMMMMMMMMMMM:{r}",
            $"{rd}:MMMMMMMMMMMMMMMMMMMMMMMM:{r}",
            $"{rd}.MMMMMMMMMMMMMMMMMMMMMMMMX.{r}",
            $"{rd} kMMMMMMMMMMMMMMMMMMMMMMMMWd.{r}",
            $"{p} .XMMMMMMMMMMMMMMMMMMMMMMMMMMk{r}",
            $"{p}  .XMMMMMMMMMMMMMMMMMMMMMMMMK.{r}",
            $"{b}    kMMMMMMMMMMMMMMMMMMMMMMd{r}",
            $"{b}     ;KMMMMMMMWXXWMMMMMMMk.{r}",
            $"{b}       .cooc,.    .,coo:.{r}",
        ];
    }

    // --- System Info Gathering ---

    private static string GetOsName()
    {
        var arch = RuntimeInformation.OSArchitecture;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return $"{RuntimeInformation.OSDescription} {arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return $"macOS {Environment.OSVersion.Version} {arch}";
        return $"{RuntimeInformation.OSDescription} {arch}";
    }

    private static string GetHostName()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var model = RunCommand("wmic", "csproduct get name /value");
                if (model is not null)
                {
                    var line = model.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .FirstOrDefault(l => l.StartsWith("Name=", StringComparison.OrdinalIgnoreCase));
                    if (line is not null)
                    {
                        var value = line["Name=".Length..].Trim();
                        if (value.Length > 0 && !value.Equals("System Product Name", StringComparison.OrdinalIgnoreCase))
                            return value;
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var vendor = ReadFileFirstLine("/sys/devices/virtual/dmi/id/board_vendor");
                var product = ReadFileFirstLine("/sys/devices/virtual/dmi/id/board_name");
                if (vendor is not null && product is not null)
                    return $"{vendor} {product}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var model = RunCommand("sysctl", "-n hw.model");
                if (model is not null)
                    return model.Trim();
            }
        }
        catch
        {
            // Fall through to default
        }

        return Environment.MachineName;
    }

    private static string GetKernelVersion() => Environment.OSVersion.ToString();

    private static string GetUptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var parts = new List<string>(3);
        if (uptime.Days > 0) parts.Add($"{uptime.Days} day{(uptime.Days != 1 ? "s" : "")}");
        if (uptime.Hours > 0) parts.Add($"{uptime.Hours} hour{(uptime.Hours != 1 ? "s" : "")}");
        parts.Add($"{uptime.Minutes} min{(uptime.Minutes != 1 ? "s" : "")}");
        return string.Join(", ", parts);
    }

    private static string GetTerminal()
    {
        // Windows Terminal
        if (Environment.GetEnvironmentVariable("WT_SESSION") is not null)
            return "Windows Terminal";

        // macOS / Linux common terminal identifiers
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        if (termProgram is not null)
            return termProgram;

        var term = Environment.GetEnvironmentVariable("TERM");
        if (term is not null)
            return term;

        return "Unknown";
    }

    private static string GetCpuName()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var id = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
                var count = Environment.ProcessorCount;
                return id is not null ? $"{id} ({count})" : $"{count} cores";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var cpuInfo = ReadFileContents("/proc/cpuinfo");
                if (cpuInfo is not null)
                {
                    var modelLine = cpuInfo.Split('\n')
                        .FirstOrDefault(l => l.StartsWith("model name", StringComparison.OrdinalIgnoreCase));
                    if (modelLine is not null)
                    {
                        var colonIdx = modelLine.IndexOf(':');
                        if (colonIdx >= 0)
                            return $"{modelLine[(colonIdx + 1)..].Trim()} ({Environment.ProcessorCount})";
                    }
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var brand = RunCommand("sysctl", "-n machdep.cpu.brand_string");
                if (brand is not null)
                    return $"{brand.Trim()} ({Environment.ProcessorCount})";
            }
        }
        catch
        {
            // Fall through to default
        }

        return $"{Environment.ProcessorCount} cores";
    }

    private static string GetMemoryInfo()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return GetWindowsMemoryInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return GetLinuxMemoryInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return GetMacOsMemoryInfo();
        }
        catch
        {
            // Fall through to default
        }

        var gcInfo = GC.GetGCMemoryInfo();
        return $"{FormatGiB(gcInfo.TotalAvailableMemoryBytes - gcInfo.HighMemoryLoadThresholdBytes)} / {FormatGiB(gcInfo.TotalAvailableMemoryBytes)}";
    }

    private static string GetWindowsMemoryInfo()
    {
        var output = RunCommand("wmic", "OS get TotalVisibleMemorySize,FreePhysicalMemory /value");
        if (output is null)
            return "Unknown";

        long totalKb = 0, freeKb = 0;
        foreach (var rawLine in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (rawLine.StartsWith("TotalVisibleMemorySize=", StringComparison.OrdinalIgnoreCase))
                long.TryParse(rawLine["TotalVisibleMemorySize=".Length..], out totalKb);
            else if (rawLine.StartsWith("FreePhysicalMemory=", StringComparison.OrdinalIgnoreCase))
                long.TryParse(rawLine["FreePhysicalMemory=".Length..], out freeKb);
        }

        if (totalKb <= 0)
            return "Unknown";

        var totalBytes = totalKb * 1024L;
        var usedBytes = (totalKb - freeKb) * 1024L;
        return $"{FormatGiB(usedBytes)} / {FormatGiB(totalBytes)}";
    }

    private static string GetLinuxMemoryInfo()
    {
        var content = ReadFileContents("/proc/meminfo");
        if (content is null) return "Unknown";

        long totalKb = 0, availableKb = 0;
        foreach (var line in content.Split('\n'))
        {
            if (line.StartsWith("MemTotal:"))
                totalKb = ParseMemInfoValue(line);
            else if (line.StartsWith("MemAvailable:"))
                availableKb = ParseMemInfoValue(line);
        }

        if (totalKb <= 0)
            return "Unknown";

        var totalBytes = totalKb * 1024L;
        var usedBytes = (totalKb - availableKb) * 1024L;
        return $"{FormatGiB(usedBytes)} / {FormatGiB(totalBytes)}";
    }

    private static string GetMacOsMemoryInfo()
    {
        var memSize = RunCommand("sysctl", "-n hw.memsize");
        if (memSize is null || !long.TryParse(memSize.Trim(), out var totalBytes))
            return "Unknown";

        // Approximate used memory via vm_stat page counts
        var vmStat = RunCommand("vm_stat", "");
        if (vmStat is not null)
        {
            long active = 0, wired = 0, compressed = 0;
            foreach (var line in vmStat.Split('\n'))
            {
                if (line.Contains("Pages active:"))
                    active = ParseVmStatPages(line);
                else if (line.Contains("Pages wired down:"))
                    wired = ParseVmStatPages(line);
                else if (line.Contains("Pages occupied by compressor:"))
                    compressed = ParseVmStatPages(line);
            }

            // macOS page size is typically 16384 on ARM, 4096 on Intel
            var pageSize = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? 16384L : 4096L;
            var usedBytes = (active + wired + compressed) * pageSize;
            return $"{FormatGiB(usedBytes)} / {FormatGiB(totalBytes)}";
        }

        return FormatGiB(totalBytes);
    }

    private static string GetDiskInfo()
    {
        try
        {
            var currentDrive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\"
                : "/";

            var drive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.IsReady && d.RootDirectory.FullName == currentDrive);

            if (drive is null)
                drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);

            if (drive is null)
                return "Unknown";

            var total = drive.TotalSize;
            var used = total - drive.TotalFreeSpace;
            var pct = total > 0 ? (int)(used * 100.0 / total) : 0;
            return $"{FormatGiB(used)} / {FormatGiB(total)} ({pct}%)";
        }
        catch
        {
            return "Unknown";
        }
    }

    private static string BuildColorRow(bool normal)
    {
        var sb = new StringBuilder();
        var reset = ThemeConfig.Reset;

        if (normal)
        {
            for (var i = 0; i < 8; i++)
                sb.Append($"\e[4{i}m   ");
        }
        else
        {
            for (var i = 0; i < 8; i++)
                sb.Append($"\e[10{i}m   ");
        }

        sb.Append(reset);
        return sb.ToString();
    }

    // --- Utility Helpers ---

    private static string FormatGiB(long bytes)
    {
        var gib = bytes / (1024.0 * 1024 * 1024);
        return $"{gib:F2} GiB";
    }

    private static long ParseMemInfoValue(string line)
    {
        // Format: "MemTotal:       16384000 kB"
        var parts = line.Split(':', 2);
        if (parts.Length < 2) return 0;
        var value = parts[1].Trim().Replace("kB", "", StringComparison.OrdinalIgnoreCase).Trim();
        return long.TryParse(value, out var result) ? result : 0;
    }

    private static long ParseVmStatPages(string line)
    {
        // Format: "Pages active:             123456."
        var colonIdx = line.IndexOf(':');
        if (colonIdx < 0) return 0;
        var value = line[(colonIdx + 1)..].Trim().TrimEnd('.');
        return long.TryParse(value, out var result) ? result : 0;
    }

    private static string? RunCommand(string fileName, string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(TimeSpan.FromSeconds(3));
            return output;
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadFileFirstLine(string path)
    {
        try
        {
            using var reader = new StreamReader(path);
            return reader.ReadLine()?.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? ReadFileContents(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return null;
        }
    }
}
