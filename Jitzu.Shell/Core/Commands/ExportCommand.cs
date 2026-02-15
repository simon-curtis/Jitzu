namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Sets environment variables.
/// </summary>
public class ExportCommand : CommandBase
{
    public ExportCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
        {
            // Show all environment variables (same as env)
            var envCommand = new EnvCommand(Context);
            return envCommand.ExecuteAsync(args);
        }

        var input = string.Join(' ', args.ToArray());
        var eqIndex = input.IndexOf('=');
        if (eqIndex <= 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: export VAR=value")));

        var name = input[..eqIndex].Trim();
        var value = input[(eqIndex + 1)..].Trim();

        // Strip surrounding quotes if present
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            value = value[1..^1];

        Environment.SetEnvironmentVariable(name, value);
        return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
    }
}
