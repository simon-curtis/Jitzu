namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Shows system uptime.
/// </summary>
public class UptimeCommand : CommandBase
{
    public UptimeCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var days = uptime.Days;
        var hours = uptime.Hours;
        var minutes = uptime.Minutes;

        var parts = new List<string>();
        if (days > 0) parts.Add($"{days} day{(days != 1 ? "s" : "")}");
        if (hours > 0) parts.Add($"{hours} hour{(hours != 1 ? "s" : "")}");
        parts.Add($"{minutes} minute{(minutes != 1 ? "s" : "")}");

        return Task.FromResult(new ShellResult(ResultType.OsCommand, $"up {string.Join(", ", parts)}", null));
    }
}
