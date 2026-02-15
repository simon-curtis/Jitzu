namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Exits the shell.
/// </summary>
public class ExitCommand : CommandBase
{
    public ExitCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        Environment.Exit(0);
        return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
    }
}
