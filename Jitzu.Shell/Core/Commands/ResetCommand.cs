namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Resets the shell session.
/// </summary>
public class ResetCommand : CommandBase
{
    public ResetCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        await Session.ResetAsync();
        return new ShellResult(ResultType.Jitzu, "Session reset.", null);
    }
}
