namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Creates an empty file or updates file timestamps.
/// </summary>
public class TouchCommand : CommandBase
{
    public TouchCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "",
                new Exception("Usage: touch [-d date] [-t YYYYMMDDhhmm] <file> [file2 ...]")));

        try
        {
            DateTime? timestamp = null;
            var files = new List<string>();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args.Span[i];
                if (arg == "-d" && i + 1 < args.Length)
                {
                    var dateStr = args.Span[++i];
                    if (!DateTime.TryParse(dateStr, out var parsed))
                        return Task.FromResult(new ShellResult(ResultType.Error, "",
                            new Exception($"Invalid date: {dateStr}")));
                    timestamp = parsed;
                }
                else if (arg == "-t" && i + 1 < args.Length)
                {
                    var timeStr = args.Span[++i];
                    if (!TryParseTouchTimestamp(timeStr, out var parsed))
                        return Task.FromResult(new ShellResult(ResultType.Error, "",
                            new Exception($"Invalid timestamp: {timeStr}. Use YYYYMMDDhhmm[.ss]")));
                    timestamp = parsed;
                }
                else
                {
                    files.Add(arg);
                }
            }

            if (files.Count == 0)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("No files specified")));

            var time = timestamp ?? DateTime.Now;

            foreach (var file in files)
            {
                var path = ExpandPath(file);
                if (File.Exists(path))
                {
                    File.SetLastWriteTime(path, time);
                    File.SetLastAccessTime(path, time);
                }
                else
                    File.Create(path).Dispose();
            }

            return Task.FromResult(new ShellResult(ResultType.Jitzu, "", null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }

    private static bool TryParseTouchTimestamp(string s, out DateTime result)
    {
        // Format: YYYYMMDDhhmm[.ss]
        result = default;
        var dotIdx = s.IndexOf('.');
        var main = dotIdx >= 0 ? s[..dotIdx] : s;
        var seconds = dotIdx >= 0 && int.TryParse(s[(dotIdx + 1)..], out var sec) ? sec : 0;

        if (main.Length != 12 || !int.TryParse(main[..4], out var year) ||
            !int.TryParse(main[4..6], out var month) || !int.TryParse(main[6..8], out var day) ||
            !int.TryParse(main[8..10], out var hour) || !int.TryParse(main[10..12], out var min))
            return false;

        try
        {
            result = new DateTime(year, month, day, hour, min, seconds);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
