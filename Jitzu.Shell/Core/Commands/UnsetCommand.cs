namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Unsets environment variables.
/// </summary>
public class UnsetCommand : CommandBase
{
    public UnsetCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: unset VAR")));

        foreach (var name in args.Span)
            Environment.SetEnvironmentVariable(name, null);

        return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
    }
}
