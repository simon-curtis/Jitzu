using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Extracts fields or characters from files.
/// </summary>
public class CutCommand : CommandBase
{
    public CutCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: cut -d <delim> -f <fields> <file>\n  -d  Delimiter (default tab)\n  -f  Field numbers (1-based, comma-separated)\n  -c  Character positions"));

        try
        {
            var delimiter = '\t';
            string? fields = null;
            string? chars = null;
            string? filePath = null;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args.Span[i];
                switch (arg)
                {
                    case "-d" when i + 1 < args.Length:
                        var d = args.Span[++i];
                        delimiter = d.Length > 0 ? d[0] : '\t';
                        break;
                    case "-f" when i + 1 < args.Length:
                        fields = args.Span[++i];
                        break;
                    case "-c" when i + 1 < args.Length:
                        chars = args.Span[++i];
                        break;
                    default:
                        filePath = arg;
                        break;
                }
            }

            if (filePath == null)
                return new ShellResult(ResultType.Error, "", new Exception("No file specified"));

            var path = ExpandPath(filePath);
            if (!File.Exists(path))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {filePath}"));

            var lines = await File.ReadAllLinesAsync(path);
            var sb = new StringBuilder();

            if (chars != null)
            {
                var positions = ParseRanges(chars);
                foreach (var line in lines)
                {
                    var selected = new StringBuilder();
                    foreach (var pos in positions)
                    {
                        if (pos - 1 < line.Length)
                            selected.Append(line[pos - 1]);
                    }

                    sb.AppendLine(selected.ToString());
                }
            }
            else if (fields != null)
            {
                var fieldIndices = ParseRanges(fields);
                foreach (var line in lines)
                {
                    var parts = line.Split(delimiter);
                    var selected = new List<string>();
                    foreach (var f in fieldIndices)
                    {
                        if (f - 1 < parts.Length)
                            selected.Add(parts[f - 1]);
                    }

                    sb.AppendLine(string.Join(delimiter, selected));
                }
            }
            else
            {
                return new ShellResult(ResultType.Error, "", new Exception("Must specify -f (fields) or -c (characters)"));
            }

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }

    private static List<int> ParseRanges(string spec)
    {
        var result = new List<int>();
        foreach (var part in spec.Split(','))
        {
            var trimmed = part.Trim();
            var dashIdx = trimmed.IndexOf('-');
            if (dashIdx > 0 && int.TryParse(trimmed[..dashIdx], out var start) && int.TryParse(trimmed[(dashIdx + 1)..], out var end))
            {
                for (var i = start; i <= end; i++)
                    result.Add(i);
            }
            else if (int.TryParse(trimmed, out var single))
            {
                result.Add(single);
            }
        }

        return result;
    }
}
