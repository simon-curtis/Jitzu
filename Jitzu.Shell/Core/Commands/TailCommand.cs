namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays the last N lines of a file.
/// </summary>
public class TailCommand : CommandBase
{
    public TailCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: tail [-n count] <file>"));

        try
        {
            var lineCount = 10;
            string? filePath = null;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args.Span[i];
                if (arg == "-n" && i + 1 < args.Length)
                {
                    if (!int.TryParse(args.Span[++i], out lineCount))
                        return new ShellResult(ResultType.Error, "", new Exception("Invalid line count"));
                }
                else
                {
                    filePath = arg;
                }
            }

            if (filePath == null)
                return new ShellResult(ResultType.Error, "", new Exception("Usage: tail [-n count] <file>"));

            var fullPath = ExpandPath(filePath);
            if (!File.Exists(fullPath))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {filePath}"));

            var allLines = await File.ReadAllLinesAsync(fullPath);
            var lines = allLines.TakeLast(lineCount);

            return new ShellResult(ResultType.OsCommand, string.Join(Environment.NewLine, lines), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
