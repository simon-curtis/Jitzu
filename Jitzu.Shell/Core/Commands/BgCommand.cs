namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Lists background jobs (same as jobs command).
/// In this shell, background jobs are already running.
/// </summary>
public class BgCommand : CommandBase
{
    public BgCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (Strategy == null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("bg: not available")));

        return Task.FromResult(Strategy.ListJobs());
    }
}
