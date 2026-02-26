using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Searches for patterns in files.
/// Supports streaming for large files to enable early termination.
/// </summary>
public class GrepCommand(CommandContext context) : CommandBase(context), IStreamingCommand
{
    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: grep [-i] [-n] [-r] [-c] <pattern> [file...]"));

        try
        {
            var (ignoreCase, showLineNumbers, recursive, countOnly, pattern, files) = ParseArgs(args);

            if (pattern == null)
                return new ShellResult(ResultType.Error, "", new Exception("No pattern specified"));

            // If no files, search current directory recursively
            if (files.Count == 0)
            {
                if (!recursive)
                    return new ShellResult(ResultType.Error, "",
                        new Exception("No files specified (use -r for recursive search)"));

                files.Add(".");
            }

            var matchRegex = BuildMatchRegex(pattern, ignoreCase);
            var fallbackComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var sb = new StringBuilder();
            var matchColor = Theme["error"]; // red for matches, like real grep
            var fileColor = Theme["ls.code"];
            const string lineNumColor = ThemeConfig.Dim;
            const string reset = ThemeConfig.Reset;
            var multiFile = files.Count > 1 || recursive;

            var filePaths = new List<string>();
            foreach (var file in files)
            {
                var expanded = ExpandPath(file);
                if (Directory.Exists(expanded))
                {
                    if (!recursive)
                    {
                        sb.AppendLine($"{Theme["error"]}grep: {file}: Is a directory{reset}");
                        continue;
                    }
                    filePaths.AddRange(Directory.GetFiles(expanded, "*", SearchOption.AllDirectories));
                }
                else if (File.Exists(expanded))
                {
                    filePaths.Add(expanded);
                }
                else
                {
                    sb.AppendLine($"{Theme["error"]}grep: {file}: No such file or directory{reset}");
                }
            }

            foreach (var filePath in filePaths)
            {
                // Skip binary files
                if (IsBinaryFile(filePath))
                    continue;

                var lines = await File.ReadAllLinesAsync(filePath);
                var fileMatchCount = 0;
                var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, filePath);

                for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
                {
                    var line = lines[lineIdx];

                    MatchCollection? lineMatches;
                    bool hasMatch;
                    try
                    {
                        lineMatches = matchRegex?.Matches(line);
                        hasMatch = matchRegex != null
                            ? lineMatches!.Count > 0
                            : line.Contains(pattern, fallbackComparison);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        continue; // Skip lines that cause catastrophic backtracking
                    }

                    if (!hasMatch) continue;

                    fileMatchCount++;

                    if (countOnly) continue;

                    var lineBuilder = new StringBuilder();

                    if (multiFile)
                        lineBuilder.Append($"{fileColor}{relativePath}{reset}:");

                    if (showLineNumbers)
                        lineBuilder.Append($"{lineNumColor}{lineIdx + 1}{reset}:");

                    AppendHighlightedLine(lineBuilder, line, pattern, lineMatches, fallbackComparison, matchColor, reset);

                    sb.AppendLine(lineBuilder.ToString());
                }

                if (countOnly && fileMatchCount > 0)
                {
                    if (multiFile)
                        sb.AppendLine($"{fileColor}{relativePath}{reset}:{fileMatchCount}");
                    else
                        sb.AppendLine(fileMatchCount.ToString());
                }
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }

    /// <summary>
    /// Streams matching lines from files, enabling early termination with head/first.
    /// </summary>
    public async IAsyncEnumerable<string> StreamAsync(
        ReadOnlyMemory<string> args,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (args.Length == 0)
            yield break;

        var (ignoreCase, showLineNumbers, recursive, _, pattern, files) = ParseArgs(args);

        if (pattern == null)
            yield break;

        if (files.Count == 0 && recursive)
            files.Add(".");
        else if (files.Count == 0)
            yield break;

        var matchRegex = BuildMatchRegex(pattern, ignoreCase);
        var fallbackComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var matchColor = Theme["error"];
        var fileColor = Theme["ls.code"];
        var lineNumColor = ThemeConfig.Dim;
        var reset = ThemeConfig.Reset;
        var multiFile = files.Count > 1 || recursive;

        // Collect all file paths
        var filePaths = new List<string>();
        foreach (var file in files)
        {
            var expanded = ExpandPath(file);
            if (Directory.Exists(expanded))
            {
                if (recursive)
                    filePaths.AddRange(Directory.GetFiles(expanded, "*", SearchOption.AllDirectories));
            }
            else if (File.Exists(expanded))
            {
                filePaths.Add(expanded);
            }
        }

        // Stream through files line-by-line
        foreach (var filePath in filePaths)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (IsBinaryFile(filePath))
                continue;

            var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, filePath);
            var lineIdx = 0;

            using var reader = new StreamReader(filePath);
            while (!cancellationToken.IsCancellationRequested
                   && await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                lineIdx++;

                MatchCollection? lineMatches;
                bool hasMatch;
                try
                {
                    lineMatches = matchRegex?.Matches(line);
                    hasMatch = matchRegex != null
                        ? lineMatches!.Count > 0
                        : line.Contains(pattern, fallbackComparison);
                }
                catch (RegexMatchTimeoutException)
                {
                    continue; // Skip lines that cause catastrophic backtracking
                }

                if (!hasMatch)
                    continue;

                var lineBuilder = new StringBuilder();

                if (multiFile)
                    lineBuilder.Append($"{fileColor}{relativePath}{reset}:");

                if (showLineNumbers)
                    lineBuilder.Append($"{lineNumColor}{lineIdx}{reset}:");

                AppendHighlightedLine(lineBuilder, line, pattern, lineMatches, fallbackComparison, matchColor, reset);

                yield return lineBuilder.ToString();
            }
        }
    }

    /// <summary>
    /// Parses grep arguments into flags, pattern, and file list.
    /// Flags must appear before the pattern. Once a non-flag argument is encountered it becomes the pattern,
    /// and all subsequent arguments are treated as file paths.
    /// </summary>
    private static (bool IgnoreCase, bool ShowLineNumbers, bool Recursive, bool CountOnly, string? Pattern, List<string> Files) ParseArgs(
        ReadOnlyMemory<string> args)
    {
        var ignoreCase = false;
        var showLineNumbers = false;
        var recursive = false;
        var countOnly = false;
        string? pattern = null;
        var files = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args.Span[i];
            if (arg.StartsWith('-') && pattern == null)
            {
                foreach (var ch in arg.AsSpan(1))
                {
                    switch (ch)
                    {
                        case 'i': ignoreCase = true; break;
                        case 'n': showLineNumbers = true; break;
                        case 'r': recursive = true; break;
                        case 'c': countOnly = true; break;
                    }
                }
            }
            else if (pattern == null)
            {
                pattern = arg;
            }
            else
            {
                files.Add(arg);
            }
        }

        return (ignoreCase, showLineNumbers, recursive, countOnly, pattern, files);
    }

    /// <summary>
    /// Builds a compiled regex for the given pattern, or returns null if the pattern is not a valid regex.
    /// </summary>
    private static Regex? BuildMatchRegex(string pattern, bool ignoreCase)
    {
        var options = ignoreCase
            ? RegexOptions.Compiled | RegexOptions.IgnoreCase
            : RegexOptions.Compiled;

        try
        {
            return new Regex(pattern, options, TimeSpan.FromMilliseconds(250));
        }
        catch (ArgumentException)
        {
            return null; // Invalid regex — caller falls back to literal substring matching
        }
    }

    /// <summary>
    /// Appends the line to the builder with all matches highlighted.
    /// Uses pre-computed regex matches when available; otherwise falls back to literal substring search.
    /// </summary>
    private static void AppendHighlightedLine(
        StringBuilder lineBuilder,
        string line,
        string pattern,
        MatchCollection? regexMatches,
        StringComparison fallbackComparison,
        string matchColor,
        string reset)
    {
        if (regexMatches != null)
        {
            var lastIndex = 0;
            foreach (Match match in regexMatches)
            {
                lineBuilder.Append(line[lastIndex..match.Index]);
                lineBuilder.Append($"{matchColor}{ThemeConfig.Bold}{match.Value}{reset}");
                lastIndex = match.Index + match.Length;
            }
            lineBuilder.Append(line[lastIndex..]);
        }
        else
        {
            // Literal fallback highlighting
            var remaining = line;
            while (true)
            {
                var idx = remaining.IndexOf(pattern, fallbackComparison);
                if (idx < 0)
                {
                    lineBuilder.Append(remaining);
                    break;
                }

                lineBuilder.Append(remaining[..idx]);
                lineBuilder.Append($"{matchColor}{ThemeConfig.Bold}{remaining[idx..(idx + pattern.Length)]}{reset}");
                remaining = remaining[(idx + pattern.Length)..];
            }
        }
    }
}
