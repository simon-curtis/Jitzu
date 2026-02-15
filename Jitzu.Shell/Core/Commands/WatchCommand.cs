namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Repeatedly executes a command at regular intervals.
/// </summary>
public class WatchCommand : CommandBase
{
    public WatchCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length == 0)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: watch [-n seconds] <command>"));

        if (Strategy == null)
            return new ShellResult(ResultType.Error, "", new Exception("watch: execution strategy not available"));

        var interval = 2.0;
        var commandParts = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args.Span[i];
            if (arg == "-n" && i + 1 < args.Length)
            {
                if (double.TryParse(args.Span[++i], out var n))
                    interval = n;
            }
            else
            {
                commandParts.Add(arg);
            }
        }

        if (commandParts.Count == 0)
            return new ShellResult(ResultType.Error, "", new Exception("No command specified"));

        var command = string.Join(' ', commandParts);
        var dim = ThemeConfig.Dim;
        var reset = ThemeConfig.Reset;

        Console.WriteLine($"{dim}Every {interval}s: {command}  (Ctrl+C to stop){reset}");
        Console.WriteLine();

        while (true)
        {
            // Clear and redraw
            Console.Clear();
            Console.WriteLine($"{dim}Every {interval}s: {command}  {DateTime.Now:HH:mm:ss}{reset}");
            Console.WriteLine();

            var result = await Strategy.ExecuteAsync(command);

            if (result.Error != null)
                Console.WriteLine($"{Theme["error"]}{result.Error.Message}{reset}");
            else if (!string.IsNullOrWhiteSpace(result.Output))
                Console.WriteLine(result.Output);

            // Wait for interval, checking for Ctrl+C
            var waitMs = (int)(interval * 1000);
            var waited = 0;
            while (waited < waitMs)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        return new ShellResult(ResultType.Jitzu, "", null);
                    if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                        return new ShellResult(ResultType.Jitzu, "", null);
                }

                await Task.Delay(100);
                waited += 100;
            }
        }
    }
}
