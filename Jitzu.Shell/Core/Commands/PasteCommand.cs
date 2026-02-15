using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Merges lines from multiple files side-by-side.
/// </summary>
public class PasteCommand : CommandBase
{
    public PasteCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: paste [-d delim] <file1> <file2> [file3 ...]"));

        try
        {
            var delimiter = "\t";
            var files = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args.Span[i];
                if (arg == "-d" && i + 1 < args.Length)
                    delimiter = args.Span[++i];
                else
                    files.Add(arg);
            }

            if (files.Count < 2)
                return new ShellResult(ResultType.Error, "", new Exception("At least two files required"));

            var allLines = new List<string[]>();
            var maxLines = 0;

            foreach (var file in files)
            {
                var path = ExpandPath(file);
                if (!File.Exists(path))
                    return new ShellResult(ResultType.Error, "", new Exception($"File not found: {file}"));

                var lines = await File.ReadAllLinesAsync(path);
                allLines.Add(lines);
                if (lines.Length > maxLines) maxLines = lines.Length;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < maxLines; i++)
            {
                for (var f = 0; f < allLines.Count; f++)
                {
                    if (f > 0) sb.Append(delimiter);
                    sb.Append(i < allLines[f].Length ? allLines[f][i] : "");
                }

                sb.AppendLine();
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
