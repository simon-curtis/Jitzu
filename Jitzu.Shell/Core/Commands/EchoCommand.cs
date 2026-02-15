namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Echoes arguments to output.
/// </summary>
public class EchoCommand : CommandBase
{
    public EchoCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        return Task.FromResult(new ShellResult(ResultType.OsCommand, string.Join(' ', args.ToArray()), null));
    }
}
