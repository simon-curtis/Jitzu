using System.Diagnostics;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Measures command execution time.
/// </summary>
public class TimeCommand : CommandBase
{
    public TimeCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: time <command>"));

        if (Strategy == null)
            return new ShellResult(ResultType.Error, "", new Exception("time: execution strategy not available"));

        var command = string.Join(' ', args.ToArray());
        var proc = Process.GetCurrentProcess();
        var userStart = proc.UserProcessorTime;
        var sysStart = proc.PrivilegedProcessorTime;

        var startTime = Stopwatch.GetTimestamp();
        var result = await Strategy.ExecuteAsync(command);
        var executionTime = Stopwatch.GetElapsedTime(startTime);

        proc.Refresh();
        var userTime = proc.UserProcessorTime - userStart;
        var sysTime = proc.PrivilegedProcessorTime - sysStart;

        var dim = ThemeConfig.Dim;
        var reset = ThemeConfig.Reset;

        // Display the command output first
        if (result.Error != null)
        {
            Console.WriteLine($"{Theme["error"]}{result.Error.Message}{reset}");
        }
        else if (!string.IsNullOrWhiteSpace(result.Output))
        {
            Console.WriteLine(result.Output);
        }

        Console.WriteLine();
        Console.WriteLine($"{dim}real{reset}    {FormatTimeDetailed(executionTime)}");
        Console.WriteLine($"{dim}user{reset}    {FormatTimeDetailed(userTime)}");
        Console.WriteLine($"{dim}sys{reset}     {FormatTimeDetailed(sysTime)}");

        return new ShellResult(ResultType.Jitzu, "", null);
    }

    private static string FormatTimeDetailed(TimeSpan ts)
        => ts.TotalMinutes >= 1
            ? $"{(int)ts.TotalMinutes}m{ts.Seconds:F3}s"
            : $"0m{ts.TotalSeconds:F3}s";
}
