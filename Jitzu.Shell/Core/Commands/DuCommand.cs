using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays disk usage of files and directories.
/// </summary>
public class DuCommand : CommandBase
{
    public DuCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        try
        {
            var summarize = false;
            var humanReadable = false;
            string? targetPath = null;

            foreach (var arg in args.Span)
            {
                if (arg.StartsWith('-'))
                {
                    foreach (var ch in arg.AsSpan(1))
                    {
                        switch (ch)
                        {
                            case 's': summarize = true; break;
                            case 'h': humanReadable = true; break;
                        }
                    }
                }
                else
                    targetPath = arg;
            }

            targetPath ??= ".";
            var fullPath = ExpandPath(targetPath);

            if (!Directory.Exists(fullPath))
            {
                if (File.Exists(fullPath))
                {
                    var size = new FileInfo(fullPath).Length;
                    var display = humanReadable ? FormatFileSize(size) : size.ToString();
                    return Task.FromResult(new ShellResult(ResultType.OsCommand, $"{display}\t{targetPath}", null));
                }

                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"No such file or directory: {targetPath}")));
            }

            var sb = new StringBuilder();
            var dim = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;

            if (summarize)
            {
                var total = GetDirectorySize(fullPath);
                var display = humanReadable ? FormatFileSize(total) : total.ToString();
                sb.Append($"{display}\t{targetPath}");
            }
            else
            {
                foreach (var dir in Directory.GetDirectories(fullPath, "*", SearchOption.AllDirectories))
                {
                    var size = GetDirectorySize(dir);
                    var relative = Path.GetRelativePath(Environment.CurrentDirectory, dir);
                    var display = humanReadable ? FormatFileSize(size).PadLeft(6) : size.ToString().PadLeft(12);
                    sb.AppendLine($"{display}\t{dim}{relative}{reset}");
                }

                var totalSize = GetDirectorySize(fullPath);
                var totalDisplay = humanReadable ? FormatFileSize(totalSize).PadLeft(6) : totalSize.ToString().PadLeft(12);
                sb.Append($"{totalDisplay}\t{targetPath}");
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
