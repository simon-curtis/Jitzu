namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Returns failure (exit code 1).
/// </summary>
public class FalseCommand : CommandBase
{
    public FalseCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("false")));
    }
}
