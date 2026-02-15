using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Translates or deletes characters from files.
/// </summary>
public class TrCommand : CommandBase
{
    public TrCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: tr [-d] <set1> [set2] <file>\n  -d  Delete characters in set1"));

        try
        {
            var delete = false;
            string? set1 = null;
            string? set2 = null;
            string? filePath = null;

            foreach (var arg in args.Span)
            {
                if (arg is "-d" or "--delete")
                    delete = true;
                else if (set1 == null)
                    set1 = arg;
                else if (!delete && set2 == null)
                    set2 = arg;
                else
                    filePath = arg;
            }

            if (set1 == null)
                return new ShellResult(ResultType.Error, "", new Exception("No character set specified"));

            if (filePath == null && !delete && set2 != null)
            {
                // Maybe set2 is actually the file if only 2 positional args given
                // Check: if set2 looks like a file path, treat it that way
                var candidatePath = ExpandPath(set2);
                if (File.Exists(candidatePath))
                {
                    filePath = set2;
                    set2 = null;
                }
            }

            if (filePath == null)
                return new ShellResult(ResultType.Error, "", new Exception("No file specified"));

            var path = ExpandPath(filePath);
            if (!File.Exists(path))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {filePath}"));

            var content = await File.ReadAllTextAsync(path);

            if (delete)
            {
                var result = new StringBuilder();
                foreach (var ch in content)
                {
                    if (!set1.Contains(ch))
                        result.Append(ch);
                }

                return new ShellResult(ResultType.OsCommand, result.ToString(), null);
            }

            if (set2 == null || set1.Length != set2.Length)
                return new ShellResult(ResultType.Error, "", new Exception("set1 and set2 must be the same length for translation"));

            var translated = new StringBuilder(content.Length);
            foreach (var ch in content)
            {
                var idx = set1.IndexOf(ch);
                translated.Append(idx >= 0 ? set2[idx] : ch);
            }

            return new ShellResult(ResultType.OsCommand, translated.ToString(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
