using System.Runtime.CompilerServices;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Writes input to file(s) and also passes it through to stdout.
/// Supports streaming to write lines incrementally.
/// </summary>
public class TeeCommand(CommandContext context) : CommandBase(context), IStreamingCommand
{
    private string? _teeInput;
    private IAsyncEnumerable<string>? _streamInput;

    /// <summary>
    /// Sets the input to be written to files and stdout.
    /// </summary>
    public void SetTeeInput(string input) => _teeInput = input;

    /// <summary>
    /// Sets streaming input for true line-by-line processing.
    /// </summary>
    public void SetStreamInput(IAsyncEnumerable<string> stream) => _streamInput = stream;

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0 && _teeInput == null && _streamInput == null)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: tee [-a] <file> [file2 ...]"));

        try
        {
            // If we have streaming input, use streaming path
            if (_streamInput != null)
            {
                var output = new System.Text.StringBuilder();
                await foreach (var line in StreamAsync(args))
                {
                    output.AppendLine(line);
                }
                return new ShellResult(ResultType.OsCommand, output.ToString().TrimEnd(), null);
            }

            var append = false;
            var files = new List<string>();

            foreach (var arg in args.Span)
            {
                if (arg is "-a" or "--append")
                    append = true;
                else
                    files.Add(arg);
            }

            if (files.Count == 0)
                return new ShellResult(ResultType.Error, "", new Exception("No output file specified"));

            var input = _teeInput ?? "";
            _teeInput = null;

            foreach (var file in files)
            {
                var path = ExpandPath(file);
                if (append)
                    await File.AppendAllTextAsync(path, input + Environment.NewLine);
                else
                    await File.WriteAllTextAsync(path, input + Environment.NewLine);
            }

            return new ShellResult(ResultType.OsCommand, input, null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }

    /// <summary>
    /// Streams lines to files and stdout incrementally.
    /// </summary>
    public async IAsyncEnumerable<string> StreamAsync(
        ReadOnlyMemory<string> args,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var append = false;
        var files = new List<string>();

        foreach (var arg in args.Span)
        {
            if (arg is "-a" or "--append")
                append = true;
            else
                files.Add(arg);
        }

        if (files.Count == 0)
            yield break;

        // Open all file streams
        var writers = new List<StreamWriter>();
        try
        {
            foreach (var file in files)
            {
                var path = ExpandPath(file);
                writers.Add(new StreamWriter(path, append: append));
            }

            // Stream lines through, writing to files and stdout
            var inputStream = _streamInput ?? StreamingPipeline.StreamFromStringAsync(_teeInput ?? "");
            _streamInput = null;
            _teeInput = null;

            await foreach (var line in inputStream.WithCancellation(cancellationToken))
            {
                // Write to all files
                foreach (var writer in writers)
                {
                    await writer.WriteLineAsync(line);
                    await writer.FlushAsync(); // Flush immediately for true streaming
                }

                // Pass through to stdout
                yield return line;
            }
        }
        finally
        {
            // Clean up all writers
            foreach (var writer in writers)
            {
                await writer.FlushAsync();
                writer.Dispose();
            }
        }
    }
}
