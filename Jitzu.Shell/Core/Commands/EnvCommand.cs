using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays environment variables.
/// </summary>
public class EnvCommand : CommandBase
{
    public EnvCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var sb = new StringBuilder();
        var vars = Environment.GetEnvironmentVariables();
        var dimColor = ThemeConfig.Dim;
        var reset = ThemeConfig.Reset;

        foreach (string key in vars.Keys.Cast<string>().OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
            sb.AppendLine($"{dimColor}{key}{reset}={vars[key]}");

        return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString(), null));
    }
}
