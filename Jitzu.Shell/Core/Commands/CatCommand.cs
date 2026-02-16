using System.Runtime.CompilerServices;
using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays file contents with line numbers.
/// Supports streaming for large files to avoid memory issues.
/// </summary>
public class CatCommand(CommandContext context) : CommandBase(context), IStreamingCommand
{
    private const long StreamingThreshold = 10_000_000; // 10MB

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: cat <file>"));

        try
        {
            var filePath = args.Span[0];
            var fullPath = ExpandPath(filePath);

            if (!File.Exists(fullPath))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {filePath}"));

            var fileInfo = new FileInfo(fullPath);

            // For large files, use streaming to avoid loading entire file
            if (fileInfo.Length > StreamingThreshold)
            {
                var sb = new StringBuilder();
                await foreach (var line in StreamAsync(args))
                {
                    sb.AppendLine(line);
                }
                return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
            }

            // For small files, use existing buffered approach
            var lines = await File.ReadAllLinesAsync(fullPath);
            var output = new StringBuilder();
            var gutterWidth = lines.Length.ToString().Length;
            var dimColor = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;

            for (var i = 0; i < lines.Length; i++)
            {
                var lineNum = (i + 1).ToString().PadLeft(gutterWidth);
                output.AppendLine($"{dimColor}{lineNum}{reset}  {lines[i]}");
            }

            return new ShellResult(ResultType.OsCommand, output.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }

    public async IAsyncEnumerable<string> StreamAsync(
        ReadOnlyMemory<string> args,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (args.Length == 0)
            yield break;

        var filePath = args.Span[0];
        var fullPath = ExpandPath(filePath);

        if (!File.Exists(fullPath))
            yield break;

        const string dimColor = ThemeConfig.Dim;
        const string reset = ThemeConfig.Reset;
        var lineNum = 0;

        // Stream line-by-line for large files
        using var reader = new StreamReader(fullPath);
        while (!cancellationToken.IsCancellationRequested && await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lineNum++;
            // Dynamic padding - use 6 digits for line numbers
            yield return $"{dimColor}{lineNum,6}{reset}  {line}";
        }
    }
}
