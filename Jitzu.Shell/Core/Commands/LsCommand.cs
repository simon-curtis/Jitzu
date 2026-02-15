using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Lists directory contents with detailed information.
/// </summary>
public class LsCommand : CommandBase
{
    public LsCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var directory = args.Length > 0
            ? ExpandPath(args.Span[0])
            : Directory.GetCurrentDirectory();

        try
        {
            var output = new StringBuilder();
            var dirInfo = new DirectoryInfo(directory);
            var entries = new List<FileSystemInfo>();

            entries.AddRange(dirInfo.GetFileSystemInfos());

            // Separate directories and files, sort each group
            var dirs = entries
                .Where(e => e is DirectoryInfo)
                .OrderBy(e => e.Name);
            var files = entries
                .Where(e => e is FileInfo)
                .OrderBy(e => e.Name);

            // Show . and .. first
            output.AppendLine(FormatEntry(dirInfo, "."));
            if (dirInfo.Parent != null)
                output.AppendLine(FormatEntry(dirInfo.Parent, ".."));

            foreach (var entry in dirs)
                output.AppendLine(FormatEntry(entry));

            foreach (var entry in files)
                output.AppendLine(FormatEntry(entry));

            return new ShellResult(
                ResultType.OsCommand,
                output.ToString(),
                null
            );
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
