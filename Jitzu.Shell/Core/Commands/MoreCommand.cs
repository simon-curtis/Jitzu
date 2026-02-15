using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Pager for viewing file contents with keyboard navigation and search.
/// Also aliased as 'less'.
/// </summary>
public class MoreCommand : CommandBase
{
    private string? _pagerInput;

    public MoreCommand(CommandContext context) : base(context) { }

    /// <summary>
    /// Sets piped input for the pager to display.
    /// </summary>
    public void SetPagerInput(string input) => _pagerInput = input;

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        string[] lines;

        if (args.Length > 0)
        {
            var path = ExpandPath(args.Span[0]);
            if (!File.Exists(path))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {args.Span[0]}"));
            lines = await File.ReadAllLinesAsync(path);
        }
        else
        {
            // When used as pipe target, input comes via PagerInput
            if (_pagerInput == null)
                return new ShellResult(ResultType.Error, "", new Exception("Usage: more <file>"));
            lines = _pagerInput.Split('\n');
            _pagerInput = null;
        }

        RunPager(lines);
        return new ShellResult(ResultType.Jitzu, "", null);
    }

    private void RunPager(string[] lines)
    {
        var offset = 0;
        var searchTerm = "";
        var dim = ThemeConfig.Dim;
        var reset = ThemeConfig.Reset;
        var highlight = Theme["error"];

        void DrawPage()
        {
            Console.Clear();
            var height = Console.WindowHeight - 1; // reserve bottom line for status
            var width = Console.WindowWidth;

            for (var i = 0; i < height && offset + i < lines.Length; i++)
            {
                var line = lines[offset + i];

                // Highlight search matches
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var sb = new StringBuilder();
                    var remaining = line;
                    while (true)
                    {
                        var idx = remaining.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
                        if (idx < 0) { sb.Append(remaining); break; }
                        sb.Append(remaining[..idx]);
                        sb.Append($"{highlight}{ThemeConfig.Bold}{remaining[idx..(idx + searchTerm.Length)]}{reset}");
                        remaining = remaining[(idx + searchTerm.Length)..];
                    }
                    line = sb.ToString();
                }

                // Truncate to terminal width (rough — doesn't account for ANSI codes)
                Console.WriteLine(line.Length > width ? line[..width] : line);
            }

            // Status bar
            var pct = lines.Length > 0 ? (int)((offset + height) * 100.0 / lines.Length) : 100;
            if (pct > 100) pct = 100;
            var status = string.IsNullOrEmpty(searchTerm)
                ? $" {dim}lines {offset + 1}-{Math.Min(offset + height, lines.Length)} of {lines.Length} ({pct}%) — q:quit /:search ↑↓:scroll{reset}"
                : $" {dim}lines {offset + 1}-{Math.Min(offset + height, lines.Length)} of {lines.Length} ({pct}%) — n:next N:prev /:search q:quit{reset}";
            Console.Write(status);
        }

        DrawPage();

        while (true)
        {
            var key = Console.ReadKey(true);
            var height = Console.WindowHeight - 1;
            var maxOffset = Math.Max(0, lines.Length - height);

            switch (key.Key)
            {
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    Console.Clear();
                    return;

                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                    if (offset < maxOffset) { offset++; DrawPage(); }
                    break;

                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                    if (offset > 0) { offset--; DrawPage(); }
                    break;

                case ConsoleKey.PageDown:
                case ConsoleKey.Spacebar:
                    offset = Math.Min(offset + height, maxOffset);
                    DrawPage();
                    break;

                case ConsoleKey.PageUp:
                case ConsoleKey.B:
                    offset = Math.Max(offset - height, 0);
                    DrawPage();
                    break;

                case ConsoleKey.Home:
                case ConsoleKey.G when key.Modifiers == ConsoleModifiers.None:
                    offset = 0;
                    DrawPage();
                    break;

                case ConsoleKey.End:
                    offset = maxOffset;
                    DrawPage();
                    break;

                case ConsoleKey.G:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        // G = go to end
                        offset = maxOffset;
                        DrawPage();
                    }
                    else
                    {
                        // g = go to top
                        offset = 0;
                        DrawPage();
                    }
                    break;

                case ConsoleKey.Oem2 when key.KeyChar == '/': // forward slash
                case ConsoleKey.Divide:
                {
                    // Search prompt
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.Write(new string(' ', Console.WindowWidth - 1));
                    Console.SetCursorPosition(0, Console.WindowHeight - 1);
                    Console.Write("/");

                    var search = new StringBuilder();
                    while (true)
                    {
                        var sk = Console.ReadKey(true);
                        if (sk.Key == ConsoleKey.Enter) break;
                        if (sk.Key == ConsoleKey.Escape) { search.Clear(); break; }
                        if (sk.Key == ConsoleKey.Backspace && search.Length > 0)
                        {
                            search.Remove(search.Length - 1, 1);
                            Console.SetCursorPosition(1, Console.WindowHeight - 1);
                            Console.Write(search + " ");
                            Console.SetCursorPosition(1 + search.Length, Console.WindowHeight - 1);
                            continue;
                        }
                        if (sk.KeyChar >= 32)
                        {
                            search.Append(sk.KeyChar);
                            Console.Write(sk.KeyChar);
                        }
                    }

                    searchTerm = search.ToString();
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        // Jump to first match from current position
                        for (var i = offset; i < lines.Length; i++)
                        {
                            if (lines[i].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            {
                                offset = Math.Min(i, maxOffset);
                                break;
                            }
                        }
                    }

                    DrawPage();
                    break;
                }

                case ConsoleKey.N:
                {
                    if (string.IsNullOrEmpty(searchTerm)) break;

                    if (key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        // N = previous match
                        for (var i = offset - 1; i >= 0; i--)
                        {
                            if (lines[i].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            {
                                offset = i;
                                DrawPage();
                                break;
                            }
                        }
                    }
                    else
                    {
                        // n = next match
                        for (var i = offset + 1; i < lines.Length; i++)
                        {
                            if (lines[i].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            {
                                offset = Math.Min(i, maxOffset);
                                DrawPage();
                                break;
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}
