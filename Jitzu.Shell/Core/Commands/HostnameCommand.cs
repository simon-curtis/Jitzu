namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays the machine hostname.
/// </summary>
public class HostnameCommand : CommandBase
{
    public HostnameCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        return Task.FromResult(new ShellResult(ResultType.OsCommand, Environment.MachineName, null));
    }
}
