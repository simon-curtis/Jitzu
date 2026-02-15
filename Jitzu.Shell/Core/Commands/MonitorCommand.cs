using Jitzu.Shell.UI.Monitor;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays a full-screen activity monitor showing CPU, RAM, and process information.
/// </summary>
public class MonitorCommand : CommandBase
{
    public MonitorCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var monitor = new ActivityMonitor(Theme);
        await monitor.RunAsync();
        return new ShellResult(ResultType.Jitzu, "", null);
    }
}
