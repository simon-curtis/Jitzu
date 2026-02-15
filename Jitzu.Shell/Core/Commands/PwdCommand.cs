namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Prints the current working directory.
/// </summary>
public class PwdCommand : CommandBase
{
    public PwdCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        return Task.FromResult(new ShellResult(ResultType.OsCommand, Directory.GetCurrentDirectory(), null));
    }
}
