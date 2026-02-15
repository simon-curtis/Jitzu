namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Repeatedly outputs a string.
/// </summary>
public class YesCommand : CommandBase
{
    public YesCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        var text = args.Length > 0 ? string.Join(' ', args.ToArray()) : "y";

        try
        {
            for (var i = 0; i < 100; i++)
            {
                Console.WriteLine(text);
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape ||
                        (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control)))
                        break;
                }

                await Task.Yield();
            }
        }
        catch (OperationCanceledException) { }

        return new ShellResult(ResultType.Jitzu, "", null);
    }
}
