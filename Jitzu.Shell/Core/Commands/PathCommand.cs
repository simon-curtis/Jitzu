namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Appends directories to the PATH environment variable.
/// Usage: path /some/dir [/another/dir ...]
/// With no arguments, displays the current PATH entries.
/// </summary>
public class PathCommand : CommandBase
{
    public PathCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";

        if (args.Length == 0)
        {
            var entries = currentPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var output = string.Join(Environment.NewLine, entries);
            return Task.FromResult(new ShellResult(ResultType.OsCommand, output, null));
        }

        var span = args.Span;
        for (var i = 0; i < span.Length; i++)
        {
            var dir = ExpandPath(span[i]);

            if (!Directory.Exists(dir))
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Directory not found: {dir}")));

            currentPath += $"{separator}{dir}";
        }

        Environment.SetEnvironmentVariable("PATH", currentPath);
        return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
    }
}
