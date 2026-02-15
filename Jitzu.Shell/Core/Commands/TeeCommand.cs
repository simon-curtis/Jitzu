namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Writes input to file(s) and also passes it through to stdout.
/// </summary>
public class TeeCommand : CommandBase
{
    private string? _teeInput;

    public TeeCommand(CommandContext context) : base(context) { }

    /// <summary>
    /// Sets the input to be written to files and stdout.
    /// </summary>
    public void SetTeeInput(string input) => _teeInput = input;

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0 && _teeInput == null)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: tee [-a] <file> [file2 ...]"));

        try
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
}
