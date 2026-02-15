namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Sorts lines in a file.
/// </summary>
public class SortCommand : CommandBase
{
    public SortCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: sort [-r] [-n] [-u] <file>"));

        try
        {
            var reverse = false;
            var numeric = false;
            var unique = false;
            string? filePath = null;

            foreach (var arg in args.Span)
            {
                if (arg.StartsWith('-') && arg.Length > 1)
                {
                    foreach (var ch in arg.AsSpan(1))
                    {
                        switch (ch)
                        {
                            case 'r': reverse = true; break;
                            case 'n': numeric = true; break;
                            case 'u': unique = true; break;
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

            IEnumerable<string> sorted;
            if (numeric)
            {
                sorted = reverse
                    ? lines.OrderByDescending(l => double.TryParse(l.TrimStart(), out var n) ? n : double.MaxValue)
                    : lines.OrderBy(l => double.TryParse(l.TrimStart(), out var n) ? n : double.MaxValue);
            }
            else
            {
                sorted = reverse
                    ? lines.OrderByDescending(l => l, StringComparer.OrdinalIgnoreCase)
                    : lines.OrderBy(l => l, StringComparer.OrdinalIgnoreCase);
            }

            if (unique)
                sorted = sorted.Distinct(StringComparer.OrdinalIgnoreCase);

            return new ShellResult(ResultType.OsCommand, string.Join(Environment.NewLine, sorted), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
