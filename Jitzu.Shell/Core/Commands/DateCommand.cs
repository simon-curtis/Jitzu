namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Displays current date and time with optional formatting.
/// </summary>
public class DateCommand : CommandBase
{
    public DateCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        try
        {
            var now = DateTime.Now;

            if (args.Length == 0)
                return Task.FromResult(new ShellResult(ResultType.OsCommand,
                    now.ToString("ddd MMM dd HH:mm:ss yyyy"), null));

            var format = args.Span[0];

            // Support common Unix format specifiers by converting to .NET
            if (format.StartsWith('+'))
            {
                var fmt = format[1..];
                fmt = fmt.Replace("%Y", "yyyy").Replace("%m", "MM").Replace("%d", "dd")
                    .Replace("%H", "HH").Replace("%M", "mm").Replace("%S", "ss")
                    .Replace("%A", "dddd").Replace("%a", "ddd").Replace("%B", "MMMM")
                    .Replace("%b", "MMM").Replace("%p", "tt").Replace("%I", "hh")
                    .Replace("%Z", "zzz").Replace("%z", "zzz").Replace("%s", "")
                    .Replace("%n", "\n").Replace("%t", "\t")
                    .Replace("%T", "HH:mm:ss").Replace("%D", "MM/dd/yy")
                    .Replace("%F", "yyyy-MM-dd").Replace("%R", "HH:mm");

                // Handle %s (Unix timestamp) specially
                if (format.Contains("%s"))
                {
                    var epoch = new DateTimeOffset(now).ToUnixTimeSeconds();
                    fmt = fmt.Length == 0 ? epoch.ToString() : fmt;
                }

                return Task.FromResult(new ShellResult(ResultType.OsCommand, now.ToString(fmt), null));
            }

            // Also accept -u for UTC
            if (format is "-u" or "--utc")
                return Task.FromResult(new ShellResult(ResultType.OsCommand,
                    DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss 'UTC' yyyy"), null));

            // Also accept -I for ISO 8601
            if (format is "-I" or "--iso-8601")
                return Task.FromResult(new ShellResult(ResultType.OsCommand,
                    now.ToString("yyyy-MM-ddTHH:mm:ssK"), null));

            return Task.FromResult(new ShellResult(ResultType.OsCommand, now.ToString(format), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
