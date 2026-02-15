namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Removes a path label.
/// </summary>
public class UnlabelCommand : CommandBase
{
    public UnlabelCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (LabelManager is null)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Label manager not available")));

        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: unlabel <name>")));

        var name = args.Span[0];
        if (LabelManager.Remove(name))
            return Task.FromResult(new ShellResult(ResultType.Jitzu, $"Label removed: {name}", null));

        return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Label not found: {name}")));
    }
}
