using System.Runtime.InteropServices;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Locates a command in the system PATH.
/// </summary>
public class WhichCommand : CommandBase
{
    public WhichCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: which <command>")));

        var command = args.Span[0];

        // Note: Cannot check builtins here without access to the command dictionary
        // This would need to be handled by the caller or passed in context

        var extensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? [".exe", ".cmd", ".bat", ".com", ".ps1"]
            : new[] { "" };

        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];

        foreach (var dir in paths)
        {
            foreach (var ext in extensions)
            {
                var candidate = Path.Combine(dir, command + ext);
                if (File.Exists(candidate))
                    return Task.FromResult(new ShellResult(ResultType.OsCommand, candidate, null));
            }
        }

        return Task.FromResult(new ShellResult(ResultType.Error, "",
            new Exception($"{command} not found")));
    }
}
