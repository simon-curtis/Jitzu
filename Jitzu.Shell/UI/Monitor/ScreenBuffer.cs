using System.Text;
using System.Text.RegularExpressions;

namespace Jitzu.Shell.UI.Monitor;

/// <summary>
/// Accumulates a full screen frame and writes it in a single Console.Write call.
/// Uses ANSI cursor-home (\e[H) instead of Console.Clear() to avoid flicker.
/// </summary>
internal sealed partial class ScreenBuffer
{
    private readonly StringBuilder _buffer = new(4096);

    [GeneratedRegex(@"\e\[[0-9;]*[A-Za-z]")]
    private static partial Regex AnsiEscapeRegex();

    public void Begin()
    {
        _buffer.Clear();
        _buffer.Append("\e[?25l"); // hide cursor
        _buffer.Append("\e[H");    // cursor home
    }

    public void AppendLine(string line, int terminalWidth)
    {
        var visible = VisibleLength(line);
        if (visible >= terminalWidth)
        {
            _buffer.Append(TruncateToWidth(line, terminalWidth));
        }
        else
        {
            _buffer.Append(line);
            _buffer.Append(' ', terminalWidth - visible);
        }
        _buffer.Append('\n');
    }

    public void AppendEmptyLine(int terminalWidth)
    {
        _buffer.Append(' ', terminalWidth);
        _buffer.Append('\n');
    }

    public void AppendRaw(string text)
    {
        _buffer.Append(text);
    }

    public void Flush()
    {
        Console.Write(_buffer.ToString());
    }

    public void ShowCursor()
    {
        Console.Write("\e[?25h");
    }

    public static int VisibleLength(string text)
    {
        return AnsiEscapeRegex().Replace(text, "").Length;
    }

    private static string TruncateToWidth(string text, int maxWidth)
    {
        var sb = new StringBuilder();
        var visible = 0;
        var i = 0;
        while (i < text.Length && visible < maxWidth)
        {
            if (text[i] == '\e' && i + 1 < text.Length && text[i + 1] == '[')
            {
                // Copy entire escape sequence
                var start = i;
                i += 2;
                while (i < text.Length && !char.IsLetter(text[i]))
                    i++;
                if (i < text.Length) i++; // include terminating letter
                sb.Append(text, start, i - start);
            }
            else
            {
                sb.Append(text[i]);
                visible++;
                i++;
            }
        }
        sb.Append("\e[0m"); // Reset to prevent color bleed into next line
        return sb.ToString();
    }
}
