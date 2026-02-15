namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Clears the console screen.
/// </summary>
public class ClearCommand : CommandBase
{
    public ClearCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        Console.Clear();
        return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
    }
}
