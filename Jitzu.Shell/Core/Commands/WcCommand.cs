using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Counts lines, words, and characters in files.
/// </summary>
public class WcCommand : CommandBase
{
    public WcCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: wc [-l] [-w] [-c] <file> [file2 ...]"));

        try
        {
            var showLines = false;
            var showWords = false;
            var showChars = false;
            var files = new List<string>();

            foreach (var arg in args.Span)
            {
                if (arg.StartsWith('-') && arg.Length > 1 && !File.Exists(ExpandPath(arg)))
                {
                    foreach (var ch in arg.AsSpan(1))
                    {
                        switch (ch)
                        {
                            case 'l': showLines = true; break;
                            case 'w': showWords = true; break;
                            case 'c': showChars = true; break;
                        }
                    }
                }
                else
                {
                    files.Add(arg);
                }
            }

            if (!showLines && !showWords && !showChars)
            {
                showLines = true;
                showWords = true;
                showChars = true;
            }

            if (files.Count == 0)
                return new ShellResult(ResultType.Error, "", new Exception("No files specified"));

            var sb = new StringBuilder();
            var totalLines = 0L;
            var totalWords = 0L;
            var totalChars = 0L;
            var dim = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;

            foreach (var file in files)
            {
                var path = ExpandPath(file);
                if (!File.Exists(path))
                {
                    sb.AppendLine($"{Theme["error"]}wc: {file}: No such file{reset}");
                    continue;
                }

                var content = await File.ReadAllTextAsync(path);
                var lineCount = content.Split('\n').Length;
                var wordCount = content.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
                var charCount = content.Length;

                totalLines += lineCount;
                totalWords += wordCount;
                totalChars += charCount;

                var parts = new List<string>();
                if (showLines) parts.Add(lineCount.ToString().PadLeft(8));
                if (showWords) parts.Add(wordCount.ToString().PadLeft(8));
                if (showChars) parts.Add(charCount.ToString().PadLeft(8));

                sb.AppendLine($"{string.Join("", parts)}  {dim}{file}{reset}");
            }

            if (files.Count > 1)
            {
                var parts = new List<string>();
                if (showLines) parts.Add(totalLines.ToString().PadLeft(8));
                if (showWords) parts.Add(totalWords.ToString().PadLeft(8));
                if (showChars) parts.Add(totalChars.ToString().PadLeft(8));
                sb.AppendLine($"{string.Join("", parts)}  total");
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }
}
