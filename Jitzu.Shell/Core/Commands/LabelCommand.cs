namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Creates or lists path labels.
/// </summary>
public class LabelCommand : CommandBase
{
    public LabelCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (LabelManager is null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Label manager not available")));

        if (args.Length == 0)
        {
            var listCommand = new ListLabelsCommand(Context);
            return listCommand.ExecuteAsync(args);
        }

        if (args.Length < 2)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: label <name> <path>")));

        var name = args.Span[0];
        var path = string.Join(' ', args.ToArray()[1..]);

        // Strip surrounding quotes if present
        if (path.Length >= 2 &&
            ((path[0] == '"' && path[^1] == '"') || (path[0] == '\'' && path[^1] == '\'')))
            path = path[1..^1];

        // Expand tilde in the target path
        if (path.StartsWith('~'))
            path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[1..]);

        LabelManager.Set(name, path);
        return Task.FromResult(new ShellResult(ResultType.Jitzu, $"Label set: {name}: → {path}", null));
    }
}
