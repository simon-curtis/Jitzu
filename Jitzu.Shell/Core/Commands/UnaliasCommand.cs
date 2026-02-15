namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Removes a command alias.
/// </summary>
public class UnaliasCommand : CommandBase
{
    public UnaliasCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (AliasManager == null)
            return new ShellResult(ResultType.Error, "", new Exception("Alias manager not available"));

        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: unalias name"));

        var name = args.Span[0];
        if (AliasManager.Remove(name))
        {
            await AliasManager.SaveAsync();
            return new ShellResult(ResultType.Jitzu, $"Alias removed: {name}", null);
        }

        return new ShellResult(ResultType.Error, "", new Exception($"Alias not found: {name}"));
    }
}
