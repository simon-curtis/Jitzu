using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Removes consecutive duplicate lines from a file.
/// </summary>
public class UniqCommand : CommandBase
{
    public UniqCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: uniq [-c] [-d] <file>"));

        try
        {
            var showCount = false;
            var duplicatesOnly = false;
            string? filePath = null;

            foreach (var arg in args.Span)
            {
                if (arg.StartsWith('-') && arg.Length > 1)
                {
                    foreach (var ch in arg.AsSpan(1))
                    {
                        switch (ch)
                        {
                            case 'c': showCount = true; break;
                            case 'd': duplicatesOnly = true; break;
                        }
                    }
                }
                else
                {
                    filePath = arg;
                }
            }

            if (filePath == null)
                return new ShellResult(ResultType.Error, "", new Exception("No file specified"));

            var path = ExpandPath(filePath);
            if (!File.Exists(path))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {filePath}"));

            var lines = await File.ReadAllLinesAsync(path);
            var sb = new StringBuilder();
            var dim = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;

            string? prev = null;
            var count = 0;

            foreach (var line in lines)
            {
                if (line == prev)
                {
                    count++;
                    continue;
                }

                if (prev != null)
                    AppendUniqLine(sb, prev, count, showCount, duplicatesOnly, dim, reset);

                prev = line;
                count = 1;
            }

            if (prev != null)
                AppendUniqLine(sb, prev, count, showCount, duplicatesOnly, dim, reset);

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }

    private static void AppendUniqLine(StringBuilder sb, string line, int count, bool showCount, bool duplicatesOnly, string dim, string reset)
    {
        if (duplicatesOnly && count < 2) return;

        if (showCount)
            sb.AppendLine($"{dim}{count,7}{reset} {line}");
        else
            sb.AppendLine(line);
    }
}
