using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Searches for patterns in files.
/// Uses parallel file processing and smart directory filtering for fast recursive search.
/// </summary>
public class GrepCommand(CommandContext context) : CommandBase(context), IStreamingCommand
{
    /// <summary>
    /// Directories skipped during recursive search (VCS internals, dependency caches, build outputs).
    /// </summary>
    private static readonly HashSet<string> SkippedDirs =
    [
        ".git", ".hg", ".svn",
        "node_modules", "vendor",
        "bin", "obj", ".vs", ".idea",
        "__pycache__", ".cache", ".gradle",
        "dist", "build", ".next", "target", "coverage"
    ];

    /// <summary>
    /// If a pattern contains none of these, it is a literal string and regex can be skipped entirely.
    /// </summary>
    private static readonly SearchValues<char> RegexMetaChars =
        SearchValues.Create(@"\.[]{}()*+?|^$");

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "",
                new Exception("Usage: grep [-i] [-n] [-r] [-c] <pattern> [file...]")));

        try
        {
            var (ignoreCase, showLineNumbers, recursive, countOnly, pattern, files) = ParseArgs(args);

            if (pattern == null)
                return Task.FromResult(new ShellResult(ResultType.Error, "",
                    new Exception("No pattern specified")));

            if (files.Count == 0)
            {
                if (!recursive)
                    return Task.FromResult(new ShellResult(ResultType.Error, "",
                        new Exception("No files specified (use -r for recursive search)")));
                files.Add(".");
            }

            var isLiteral = !pattern.AsSpan().ContainsAny(RegexMetaChars);
            var matchRegex = isLiteral ? null : BuildMatchRegex(pattern, ignoreCase);
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var matchColor = Theme["error"];
            var fileColor = Theme["ls.code"];
            const string lineNumColor = ThemeConfig.Dim;
            const string reset = ThemeConfig.Reset;

            var sb = new StringBuilder();
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

                    EnumerateFilesRecursive(expanded, filePaths);
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

            var multiFile = filePaths.Count > 1;
            var cwd = Environment.CurrentDirectory;

            if (filePaths.Count <= 1)
            {
                foreach (var filePath in filePaths)
                {
                    var result = SearchFile(filePath, cwd, matchRegex, pattern, comparison,
                        showLineNumbers, multiFile, countOnly, matchColor, fileColor, lineNumColor, reset);
                    if (result != null) sb.Append(result);
                }
            }
            else
            {
                var results = new string?[filePaths.Count];
                Parallel.For(0, filePaths.Count,
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    i =>
                    {
                        results[i] = SearchFile(filePaths[i], cwd, matchRegex, pattern, comparison,
                            showLineNumbers, multiFile, countOnly, matchColor, fileColor, lineNumColor, reset);
                    });

                foreach (var result in results)
                    if (result != null)
                        sb.Append(result);
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
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

        var isLiteral = !pattern.AsSpan().ContainsAny(RegexMetaChars);
        var matchRegex = isLiteral ? null : BuildMatchRegex(pattern, ignoreCase);
        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var matchColor = Theme["error"];
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
                if (recursive)
                    EnumerateFilesRecursive(expanded, filePaths);
            }
            else if (File.Exists(expanded))
            {
                filePaths.Add(expanded);
            }
        }

        var lineBuilder = new StringBuilder();
        var header = new byte[512];

        foreach (var filePath in filePaths)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            FileStream fs;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, bufferSize: 1, FileOptions.SequentialScan);
            }
            catch { continue; }

            using (fs)
            {
                // Inline binary check
                var headerRead = fs.Read(header);
                var isBinary = false;
                for (var i = 0; i < headerRead; i++)
                {
                    if (header[i] != 0) continue;
                    isBinary = true;
                    break;
                }

                if (isBinary) continue;
                fs.Position = 0;

                using var reader = new StreamReader(fs, Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true, bufferSize: 65536);
                var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, filePath);
                var lineNum = 0;

                while (!cancellationToken.IsCancellationRequested
                       && await reader.ReadLineAsync(cancellationToken) is { } line)
                {
                    lineNum++;

                    bool hasMatch;
                    MatchCollection? lineMatches = null;

                    if (matchRegex != null)
                    {
                        try
                        {
                            hasMatch = matchRegex.IsMatch(line);
                            if (hasMatch) lineMatches = matchRegex.Matches(line);
                        }
                        catch (RegexMatchTimeoutException) { continue; }
                    }
                    else
                    {
                        hasMatch = line.Contains(pattern, comparison);
                    }

                    if (!hasMatch) continue;

                    lineBuilder.Clear();
                    if (multiFile) lineBuilder.Append(fileColor).Append(relativePath).Append(reset).Append(':');
                    if (showLineNumbers) lineBuilder.Append(lineNumColor).Append(lineNum).Append(reset).Append(':');
                    AppendHighlightedLine(lineBuilder, line, pattern, lineMatches, comparison, matchColor, reset);

                    yield return lineBuilder.ToString();
                }
            }
        }
    }

    /// <summary>
    /// Searches a single file for matching lines. Returns formatted output or null if no matches / binary.
    /// Opens the file once with SequentialScan and performs the binary check inline.
    /// </summary>
    private static string? SearchFile(
        string filePath, string cwd,
        Regex? matchRegex, string pattern, StringComparison comparison,
        bool showLineNumbers, bool multiFile, bool countOnly,
        string matchColor, string fileColor, string lineNumColor, string reset)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                FileShare.Read, bufferSize: 1, FileOptions.SequentialScan);

            // Inline binary check — avoids a separate file open
            Span<byte> header = stackalloc byte[512];
            var headerRead = fs.Read(header);
            for (var i = 0; i < headerRead; i++)
                if (header[i] == 0) return null;
            fs.Position = 0;

            using var reader = new StreamReader(fs, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 65536);
            var relativePath = Path.GetRelativePath(cwd, filePath);
            StringBuilder? sb = null;
            var lineNum = 0;
            var matchCount = 0;

            while (reader.ReadLine() is { } line)
            {
                lineNum++;

                bool hasMatch;
                MatchCollection? lineMatches = null;

                if (matchRegex != null)
                {
                    try
                    {
                        hasMatch = matchRegex.IsMatch(line);
                        if (hasMatch && !countOnly)
                            lineMatches = matchRegex.Matches(line);
                    }
                    catch (RegexMatchTimeoutException) { continue; }
                }
                else
                {
                    hasMatch = line.Contains(pattern, comparison);
                }

                if (!hasMatch) continue;
                matchCount++;
                if (countOnly) continue;

                sb ??= new StringBuilder();
                if (multiFile) sb.Append(fileColor).Append(relativePath).Append(reset).Append(':');
                if (showLineNumbers) sb.Append(lineNumColor).Append(lineNum).Append(reset).Append(':');
                AppendHighlightedLine(sb, line, pattern, lineMatches, comparison, matchColor, reset);
                sb.AppendLine();
            }

            if (countOnly && matchCount > 0)
            {
                sb ??= new StringBuilder();
                if (multiFile) sb.Append(fileColor).Append(relativePath).Append(reset).Append(':');
                sb.AppendLine(matchCount.ToString());
            }

            return sb?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Recursively enumerates files, skipping VCS internals, dependency caches, and build output directories.
    /// </summary>
    private static void EnumerateFilesRecursive(string root, List<string> results)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(root))
                results.Add(file);

            foreach (var dir in Directory.EnumerateDirectories(root))
            {
                if (!SkippedDirs.Contains(Path.GetFileName(dir)))
                    EnumerateFilesRecursive(dir, results);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
    }

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
            return null;
        }
    }

    /// <summary>
    /// Appends the line with matches highlighted. Uses span-based appends to avoid substring allocations.
    /// </summary>
    private static void AppendHighlightedLine(
        StringBuilder sb,
        string line,
        string pattern,
        MatchCollection? regexMatches,
        StringComparison comparison,
        string matchColor,
        string reset)
    {
        if (regexMatches != null)
        {
            var lastIndex = 0;
            foreach (Match match in regexMatches)
            {
                sb.Append(line.AsSpan(lastIndex, match.Index - lastIndex));
                sb.Append(matchColor).Append(ThemeConfig.Bold);
                sb.Append(line.AsSpan(match.Index, match.Length));
                sb.Append(reset);
                lastIndex = match.Index + match.Length;
            }

            sb.Append(line.AsSpan(lastIndex));
        }
        else
        {
            var pos = 0;
            while (pos < line.Length)
            {
                var idx = line.IndexOf(pattern, pos, comparison);
                if (idx < 0)
                {
                    sb.Append(line.AsSpan(pos));
                    break;
                }

                sb.Append(line.AsSpan(pos, idx - pos));
                sb.Append(matchColor).Append(ThemeConfig.Bold);
                sb.Append(line.AsSpan(idx, pattern.Length));
                sb.Append(reset);
                pos = idx + pattern.Length;
            }
        }
    }
}
