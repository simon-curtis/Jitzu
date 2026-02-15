using System.Management;
using System.Runtime.InteropServices;

namespace Jitzu.Benchmarking.Display;

public static class SystemInfoCollector
{
    public static string GetSystemInfo()
    {
        var osInfo = GetOperatingSystemInfo();
        var cpuInfo = GetProcessorInfo();

        return $"{osInfo}\n{cpuInfo}";
    }

    private static string GetOperatingSystemInfo()
    {
        var osVersion = Environment.OSVersion;
        var osName = GetWindowsVersionName();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $"OS=Windows {osVersion.Version} ({osName})";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"OS=Linux {osVersion.Version}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"OS=macOS {osVersion.Version}";
        }

        return $"OS={RuntimeInformation.OSDescription}";
    }

    private static string GetWindowsVersionName()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return string.Empty;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");

            foreach (ManagementObject os in searcher.Get())
            {
                var caption = os["Caption"]?.ToString();
                if (!string.IsNullOrEmpty(caption))
                {
                    // Extract version info from caption
                    if (caption.Contains("Windows 11"))
                        return "21H2/Windows11";
                    if (caption.Contains("Windows 10"))
                    {
                        var version = Environment.OSVersion.Version;
                        return version.Build switch
                        {
                            >= 22000 => "21H2/Windows11",
                            >= 19044 => "21H2/November2021Update",
                            >= 19043 => "21H1/May2021Update",
                            >= 19042 => "20H2/October2020Update",
                            >= 19041 => "2004/May2020Update",
                            >= 18363 => "1909/November2019Update",
                            >= 18362 => "1903/May2019Update",
                            >= 17763 => "1809/October2018Update/Redstone5",
                            _ => "Unknown"
                        };
                    }
                }
            }
        }
        catch
        {
            // Fallback if WMI fails
        }

        return "Unknown";
    }

    private static string GetProcessorInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");

            foreach (ManagementObject processor in searcher.Get())
            {
                var name = processor["Name"]?.ToString()?.Trim();
                var maxClockSpeed = Convert.ToUInt32(processor["MaxClockSpeed"]);
                var physicalCores = Convert.ToInt32(processor["NumberOfCores"]);
                var logicalCores = Convert.ToInt32(processor["NumberOfLogicalProcessors"]);

                // Clean up processor name
                name = CleanProcessorName(name);

                // Get architecture info
                var architecture = GetProcessorArchitecture(name);

                var clockSpeedGHz = maxClockSpeed / 1000.0;

                return $"{name} CPU {clockSpeedGHz:F2}GHz ({architecture}), " +
                       $"1 CPU, {logicalCores} logical and {physicalCores} physical cores";
            }
        }
        catch
        {
            // Fallback
            return $"CPU information unavailable, " +
                   $"{Environment.ProcessorCount} logical cores";
        }

        return "Unknown CPU";
    }

    private static string CleanProcessorName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "Unknown Processor";

        // Remove extra spaces and common suffixes
        name = name.Replace("(R)", "")
            .Replace("(TM)", "")
            .Replace("  ", " ")
            .Trim();

        return name;
    }

    private static string GetProcessorArchitecture(string processorName)
    {
        if (string.IsNullOrEmpty(processorName))
            return "Unknown";

        var name = processorName.ToLower();

        // Intel architectures
        if (name.Contains("i3-13") || name.Contains("i5-13") || name.Contains("i7-13") || name.Contains("i9-13"))
            return "Raptor Lake";
        if (name.Contains("i3-12") || name.Contains("i5-12") || name.Contains("i7-12") || name.Contains("i9-12"))
            return "Alder Lake";
        if (name.Contains("i3-11") || name.Contains("i5-11") || name.Contains("i7-11") || name.Contains("i9-11"))
            return "Tiger Lake";
        if (name.Contains("i3-10") || name.Contains("i5-10") || name.Contains("i7-10") || name.Contains("i9-10"))
            return "Comet Lake";
        if (name.Contains("i3-9") || name.Contains("i5-9") || name.Contains("i7-9") || name.Contains("i9-9"))
            return "Coffee Lake";
        if (name.Contains("i3-8") || name.Contains("i5-8") || name.Contains("i7-8"))
            return "Coffee Lake";
        if (name.Contains("i3-7") || name.Contains("i5-7") || name.Contains("i7-7"))
            return "Kaby Lake";
        if (name.Contains("i3-6") || name.Contains("i5-6") || name.Contains("i7-6"))
            return "Skylake";

        // AMD architectures
        if (name.Contains("ryzen") && name.Contains("7000"))
            return "Zen 4";
        if (name.Contains("ryzen") && name.Contains("5000"))
            return "Zen 3";
        if (name.Contains("ryzen") && name.Contains("4000"))
            return "Zen 2";
        if (name.Contains("ryzen") && name.Contains("3000"))
            return "Zen 2";
        if (name.Contains("ryzen") && name.Contains("2000"))
            return "Zen+";
        if (name.Contains("ryzen") && name.Contains("1000"))
            return "Zen";

        return "Unknown";
    }
}