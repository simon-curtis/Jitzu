namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Strips last component from file path.
/// </summary>
public class DirnameCommand : CommandBase
{
    public DirnameCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: dirname <path>")));

        var dir = Path.GetDirectoryName(args.Span[0]) ?? ".";
        return Task.FromResult(new ShellResult(ResultType.OsCommand, dir, null));
    }
}
