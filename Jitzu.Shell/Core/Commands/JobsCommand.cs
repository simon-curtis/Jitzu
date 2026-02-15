namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Lists background jobs.
/// </summary>
public class JobsCommand : CommandBase
{
    public JobsCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (Strategy == null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("jobs: not available")));

        return Task.FromResult(Strategy.ListJobs());
    }
}
