using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Prints a sequence of numbers.
/// </summary>
public class SeqCommand : CommandBase
{
    public SeqCommand(CommandContext context) : base(context) { }

    public override Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Usage: seq [first [increment]] last")));

        try
        {
            double first = 1, increment = 1, last;

            if (args.Length == 1)
            {
                if (!double.TryParse(args.Span[0], out last))
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception($"Invalid number: {args.Span[0]}")));
            }
            else if (args.Length == 2)
            {
                if (!double.TryParse(args.Span[0], out first) || !double.TryParse(args.Span[1], out last))
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Invalid numbers")));
            }
            else
            {
                if (!double.TryParse(args.Span[0], out first) || !double.TryParse(args.Span[1], out increment) || !double.TryParse(args.Span[2], out last))
                    return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Invalid numbers")));
            }

            if (increment == 0)
                return Task.FromResult(new ShellResult(ResultType.Error, "", new Exception("Increment cannot be zero")));

            var sb = new StringBuilder();
            var count = 0;
            var isInt = first == Math.Floor(first) && increment == Math.Floor(increment) && last == Math.Floor(last);

            for (var i = first; increment > 0 ? i <= last : i >= last; i += increment)
            {
                sb.AppendLine(isInt ? ((int)i).ToString() : i.ToString("G"));
                if (++count > 10000)
                {
                    sb.AppendLine("... (truncated at 10000)");
                    break;
                }
            }

            return Task.FromResult(new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ShellResult(ResultType.Error, "", ex));
        }
    }
}
