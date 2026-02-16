using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Jitzu.Shell.Core;

/// <summary>
/// Provides streaming pipeline functionality for efficient data processing.
/// Uses IAsyncEnumerable for line-by-line streaming to avoid buffering entire outputs.
/// </summary>
public static class StreamingPipeline
{
    /// <summary>
    /// Streams lines from a process stdout asynchronously.
    /// </summary>
    public static async IAsyncEnumerable<string> StreamFromProcessAsync(
        Process process,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = process.StandardOutput;

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null)
                break;

            yield return line;
        }

        // If cancelled, kill the process to avoid zombie processes
        if (cancellationToken.IsCancellationRequested && !process.HasExited)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Process may have already exited
            }
        }
    }

    /// <summary>
    /// Streams lines from a string input.
    /// </summary>
    public static async IAsyncEnumerable<string> StreamFromStringAsync(
        string input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(input);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null)
                break;

            yield return line;
        }
    }

    /// <summary>
    /// Materializes a stream to a string (for final output).
    /// </summary>
    public static async Task<string> MaterializeAsync(
        IAsyncEnumerable<string> stream,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var first = true;

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            if (!first)
                sb.Append('\n');
            sb.Append(line);
            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Materializes a stream to a string array (for compatibility).
    /// </summary>
    public static async Task<string[]> MaterializeToArrayAsync(
        IAsyncEnumerable<string> stream,
        CancellationToken cancellationToken = default)
    {
        var lines = new List<string>();

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            lines.Add(line);
        }

        return lines.ToArray();
    }
}

/// <summary>
/// Streaming versions of pipe functions that operate on IAsyncEnumerable<string>.
/// These functions process data line-by-line without buffering entire outputs.
/// </summary>
public static class StreamingPipeFunctions
{
    /// <summary>
    /// Returns the first line from the stream.
    /// Supports early termination - stops reading after the first line.
    /// </summary>
    public static async IAsyncEnumerable<string> FirstAsync(
        IAsyncEnumerable<string> stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            yield return line;
            yield break; // Early termination - only need first line
        }
    }

    /// <summary>
    /// Returns the last line from the stream.
    /// Note: Must consume entire stream to find the last line.
    /// </summary>
    public static async IAsyncEnumerable<string> LastAsync(
        IAsyncEnumerable<string> stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? lastLine = null;

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            lastLine = line;
        }

        if (lastLine != null)
            yield return lastLine;
    }

    /// <summary>
    /// Returns the nth line from the stream (0-indexed).
    /// Supports early termination - stops after reading the nth line.
    /// </summary>
    public static async IAsyncEnumerable<string> NthAsync(
        IAsyncEnumerable<string> stream,
        int index,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (index < 0)
            yield break;

        var currentIndex = 0;

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            if (currentIndex == index)
            {
                yield return line;
                yield break; // Early termination
            }
            currentIndex++;
        }
    }

    /// <summary>
    /// Filters lines containing the pattern (case-insensitive).
    /// Streams results as matches are found.
    /// </summary>
    public static async IAsyncEnumerable<string> GrepAsync(
        IAsyncEnumerable<string> stream,
        string pattern,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            if (line.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                yield return line;
        }
    }

    /// <summary>
    /// Returns the first N lines from the stream.
    /// Supports early termination - stops after N lines.
    /// </summary>
    public static async IAsyncEnumerable<string> HeadAsync(
        IAsyncEnumerable<string> stream,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            yield break;

        var linesReturned = 0;

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            yield return line;
            linesReturned++;

            if (linesReturned >= count)
                yield break; // Early termination
        }
    }

    /// <summary>
    /// Returns the last N lines from the stream.
    /// Note: Must buffer last N lines in memory.
    /// </summary>
    public static async IAsyncEnumerable<string> TailAsync(
        IAsyncEnumerable<string> stream,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            yield break;

        var buffer = new Queue<string>(count + 1);

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            buffer.Enqueue(line);
            if (buffer.Count > count)
                buffer.Dequeue();
        }

        foreach (var line in buffer)
            yield return line;
    }

    /// <summary>
    /// Sorts all lines alphabetically.
    /// Note: Must buffer entire stream for sorting.
    /// </summary>
    public static async IAsyncEnumerable<string> SortAsync(
        IAsyncEnumerable<string> stream,
        bool reverse = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lines = new List<string>();

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            lines.Add(line);
        }

        lines.Sort(StringComparer.OrdinalIgnoreCase);

        if (reverse)
            lines.Reverse();

        foreach (var line in lines)
            yield return line;
    }

    /// <summary>
    /// Removes consecutive duplicate lines.
    /// Only needs to remember the previous line.
    /// </summary>
    public static async IAsyncEnumerable<string> UniqAsync(
        IAsyncEnumerable<string> stream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? previous = null;

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            if (line != previous)
            {
                yield return line;
                previous = line;
            }
        }
    }

    /// <summary>
    /// Counts lines, words, and characters in the stream.
    /// Returns a single line with the counts.
    /// </summary>
    public static async IAsyncEnumerable<string> WcAsync(
        IAsyncEnumerable<string> stream,
        bool linesOnly = false,
        bool wordsOnly = false,
        bool charsOnly = false,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lineCount = 0;
        var wordCount = 0;
        var charCount = 0;

        await foreach (var line in stream.WithCancellation(cancellationToken))
        {
            lineCount++;
            charCount += line.Length + 1; // +1 for newline

            // Count words (split by whitespace)
            var words = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            wordCount += words.Length;
        }

        // Format output based on flags
        if (!linesOnly && !wordsOnly && !charsOnly)
        {
            yield return $"{lineCount}\t{wordCount}\t{charCount}";
        }
        else
        {
            var parts = new List<string>();
            if (linesOnly) parts.Add(lineCount.ToString());
            if (wordsOnly) parts.Add(wordCount.ToString());
            if (charsOnly) parts.Add(charCount.ToString());
            yield return string.Join('\t', parts);
        }
    }

    /// <summary>
    /// Prints lines to console and passes them through.
    /// </summary>
    public static async IAsyncEnumerable<string> TeeAsync(
        IAsyncEnumerable<string> stream,
        string? outputFile = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        StreamWriter? fileWriter = null;

        try
        {
            if (outputFile != null)
                fileWriter = new StreamWriter(outputFile, append: false);

            await foreach (var line in stream.WithCancellation(cancellationToken))
            {
                Console.WriteLine(line);

                if (fileWriter != null)
                    await fileWriter.WriteLineAsync(line);

                yield return line;
            }
        }
        finally
        {
            if (fileWriter != null)
            {
                await fileWriter.FlushAsync();
                fileWriter.Dispose();
            }
        }
    }
}
