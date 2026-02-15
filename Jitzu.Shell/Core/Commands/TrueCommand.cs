namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Returns success (exit code 0).
/// </summary>
public class TrueCommand : CommandBase
{
    public TrueCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
    }
}
