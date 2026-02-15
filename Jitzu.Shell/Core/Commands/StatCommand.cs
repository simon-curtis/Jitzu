using System.Runtime.InteropServices;
using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays detailed file or directory metadata.
/// </summary>
public class StatCommand : CommandBase
{
    public StatCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: stat <file> [file2 ...]")));

        try
        {
            var sb = new StringBuilder();
            var reset = ThemeConfig.Reset;
            var label = Theme["ls.config"];

            foreach (var arg in args.Span)
            {
                var path = ExpandPath(arg);
                FileSystemInfo? info = null;

                if (File.Exists(path))
                    info = new FileInfo(path);
                else if (Directory.Exists(path))
                    info = new DirectoryInfo(path);

                if (info == null)
                {
                    sb.AppendLine($"stat: cannot stat '{arg}': No such file or directory");
                    continue;
                }

                sb.AppendLine($"{label}  File:{reset} {arg}");

                if (info is FileInfo fi)
                    sb.AppendLine($"{label}  Size:{reset} {FormatFileSize(fi.Length)} ({fi.Length} bytes)");

                var fileType = info switch
                {
                    _ when info.LinkTarget != null => "symbolic link",
                    DirectoryInfo => "directory",
                    _ => "regular file"
                };
                sb.AppendLine($"{label}  Type:{reset} {fileType}");

                if (info.LinkTarget != null)
                    sb.AppendLine($"{label}Target:{reset} {info.LinkTarget}");

                sb.AppendLine($"{label}Access:{reset} {info.LastAccessTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"{label}Modify:{reset} {info.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"{label}Create:{reset} {info.CreationTime:yyyy-MM-dd HH:mm:ss}");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var attrs = new List<string>();
                    if (info.Attributes.HasFlag(FileAttributes.ReadOnly)) attrs.Add("ReadOnly");
                    if (info.Attributes.HasFlag(FileAttributes.Hidden)) attrs.Add("Hidden");
                    if (info.Attributes.HasFlag(FileAttributes.System)) attrs.Add("System");
                    if (info.Attributes.HasFlag(FileAttributes.Archive)) attrs.Add("Archive");
                    if (info.Attributes.HasFlag(FileAttributes.ReparsePoint)) attrs.Add("ReparsePoint");
                    sb.AppendLine($"{label} Attrs:{reset} {(attrs.Count > 0 ? string.Join(", ", attrs) : "None")}");
                }

                sb.AppendLine();
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
