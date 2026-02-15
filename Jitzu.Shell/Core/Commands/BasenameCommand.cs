namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Strips directory and optionally suffix from file path.
/// </summary>
public class BasenameCommand : CommandBase
{
    public BasenameCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: basename <path> [suffix]")));

        var name = Path.GetFileName(args.Span[0]);
        if (args.Length > 1)
        {
            var suffix = args.Span[1];
            if (name.EndsWith(suffix, StringComparison.Ordinal))
                name = name[..^suffix.Length];
        }

        return Task.FromResult(new ShellResult(ResultType.OsCommand, name, null));
    }
}
