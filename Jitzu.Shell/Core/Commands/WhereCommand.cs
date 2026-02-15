using System.Runtime.InteropServices;

namespace Jitzu.Shell.Core.Commands;

public class WhereCommand : CommandBase
{
    public WhereCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: where <command>")));

        var command = args.Span[0];
        var results = new List<string>();

        if (AliasManager is { } aliasManager)
        {
            var aliases = aliasManager.Aliases;
            if (aliases.TryGetValue(command, out var aliasValue))
                results.Add($"alias: {command}={aliasValue}");
        }

        if (LabelManager is { } labelManager)
        {
            var labels = labelManager.Labels;
            if (labels.TryGetValue(command, out var labelValue))
                results.Add($"label: {command}={labelValue}");
        }

        if (Context.BuiltinCommands?.IsBuiltin(command) == true)
            results.Add($"builtin: {command}");

        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        var extensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? [".exe", ".cmd", ".bat", ".com", ".ps1"]
            : new[] { "" };

        foreach (var dir in paths)
        {
            foreach (var ext in extensions)
            {
                var candidate = Path.Combine(dir, command + ext);
                if (File.Exists(candidate))
                {
                    results.Add($"PATH: {candidate}");
                    break;
                }
            }
        }

        if (results.Count == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "",
                new Exception($"{command} not found")));

        return Task.FromResult(new ShellResult(ResultType.OsCommand, string.Join(Environment.NewLine, results), null));
    }
}
