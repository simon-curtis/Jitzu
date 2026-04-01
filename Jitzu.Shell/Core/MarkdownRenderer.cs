using System.Text;
using System.Text.RegularExpressions;

namespace Jitzu.Shell.Core;

/// <summary>
/// Renders markdown lines into ANSI-formatted lines for terminal display.
/// </summary>
public static partial class MarkdownRenderer
{
    private const string Reset = "\e[0m";
    private const string Bold = "\e[1m";
    private const string Italic = "\e[3m";
    private const string Dim = "\e[2m";
    private const string Underline = "\e[4m";
    private const string H1Color = "\e[38;2;135;175;215m"; // blue
    private const string H2Color = "\e[38;2;135;175;135m"; // green
    private const string H3Color = "\e[38;2;215;175;135m"; // warm
    private const string CodeColor = "\e[38;2;210;210;160m"; // warm yellow
    private const string QuoteColor = "\e[38;2;128;128;128m"; // gray
    private const string LinkColor = "\e[38;2;135;175;175m"; // teal
    private const string RuleColor = "\e[38;2;80;80;80m"; // dark gray
    private const string TableBorder = "\e[38;2;80;80;80m"; // dark gray
    private const string BulletColor = "\e[38;2;135;175;135m"; // green
    private const string NumColor = "\e[38;2;135;175;215m"; // blue

    private const int RuleWidth = 40;

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex BoldPattern();

    [GeneratedRegex(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)")]
    private static partial Regex ItalicPattern();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex InlineCodePattern();

    [GeneratedRegex(@"\[([^\]]+)\]\(([^)]+)\)")]
    private static partial Regex LinkPattern();

    [GeneratedRegex(@"^(\d+)\.\s+(.+)$")]
    private static partial Regex NumberedListPattern();

    [GeneratedRegex(@"^\|[\s:]*-{3,}[\s:]*")]
    private static partial Regex TableSeparatorPattern();

    [GeneratedRegex(@"\e\[[^m]*m")]
    private static partial Regex AnsiEscapePattern();

    public static string[] Render(string[] lines)
    {
        var result = new List<string>();
        var inCodeBlock = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Code block toggle
            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    inCodeBlock = true;
                    result.Add($"{Dim}{"".PadRight(RuleWidth, '─')}{Reset}");
                    continue;
                }

                inCodeBlock = false;
                result.Add($"{Dim}{"".PadRight(RuleWidth, '─')}{Reset}");
                continue;
            }

            if (inCodeBlock)
            {
                result.Add($"  {CodeColor}{line}{Reset}");
                continue;
            }

            // Table detection — look ahead for separator row
            if (IsTableRow(line) && i + 1 < lines.Length && TableSeparatorPattern().IsMatch(lines[i + 1].TrimEnd()))
            {
                i = RenderTable(lines, i, result);
                continue;
            }

            // Headers
            if (line.StartsWith("### "))
            {
                result.Add($"{Bold}{H3Color}{line[4..].Trim()}{Reset}");
                continue;
            }

            if (line.StartsWith("## "))
            {
                result.Add($"{Bold}{H2Color}{line[3..].Trim()}{Reset}");
                continue;
            }

            if (line.StartsWith("# "))
            {
                result.Add($"{Bold}{H1Color}{line[2..].Trim()}{Reset}");
                continue;
            }

            // Horizontal rule
            if (line is "---" or "***" or "___")
            {
                result.Add($"{RuleColor}{"".PadRight(RuleWidth, '─')}{Reset}");
                continue;
            }

            // Blockquote
            if (line.StartsWith("> "))
            {
                var content = FormatInline(line[2..]);
                result.Add($"  {QuoteColor}│{Reset} {QuoteColor}{content}{Reset}");
                continue;
            }

            // Bullet list
            if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                var content = FormatInline(line[2..]);
                result.Add($"  {BulletColor}•{Reset} {content}");
                continue;
            }

            // Numbered list
            var numMatch = NumberedListPattern().Match(line);
            if (numMatch.Success)
            {
                var num = numMatch.Groups[1].Value;
                var content = FormatInline(numMatch.Groups[2].Value);
                result.Add($"  {NumColor}{num}.{Reset} {content}");
                continue;
            }

            // Plain line with inline formatting
            result.Add(FormatInline(line));
        }

        return [.. result];
    }

    private static string FormatInline(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Order matters: bold before italic (** before *)
        text = BoldPattern().Replace(text, m => $"{Bold}{m.Groups[1].Value}{Reset}");
        text = ItalicPattern().Replace(text, m => $"{Italic}{m.Groups[1].Value}{Reset}");
        text = InlineCodePattern().Replace(text, m => $"{CodeColor}{m.Groups[1].Value}{Reset}");
        text = LinkPattern().Replace(text, m => $"{Underline}{m.Groups[1].Value}{Reset} {Dim}({m.Groups[2].Value}){Reset}");

        return text;
    }

    /// <summary>
    /// Checks if a line looks like a markdown table row (starts with |).
    /// </summary>
    private static bool IsTableRow(string line) => line.TrimStart().StartsWith('|');

    /// <summary>
    /// Parses a table row into trimmed cells, tolerating missing trailing pipe and trailing whitespace.
    /// </summary>
    private static string[] ParseTableCells(string line)
    {
        line = line.Trim();

        // Strip leading pipe
        if (line.StartsWith('|')) line = line[1..];
        // Strip trailing pipe
        if (line.EndsWith('|')) line = line[..^1];

        return line.Split('|').Select(c => c.Trim()).ToArray();
    }

    /// <summary>
    /// Returns the visible length of a string, ignoring ANSI escape codes.
    /// </summary>
    private static int VisibleLength(string text) => AnsiEscapePattern().Replace(text, "").Length;

    /// <summary>
    /// Pads a string containing ANSI codes to a target visible width.
    /// </summary>
    private static string PadRightVisible(string text, int targetWidth)
    {
        var visible = VisibleLength(text);
        return visible >= targetWidth ? text : text + new string(' ', targetWidth - visible);
    }

    /// <summary>
    /// Renders a markdown table block starting at index i. Returns the last consumed index.
    /// </summary>
    private static int RenderTable(string[] lines, int start, List<string> result)
    {
        // Parse all table rows (plain text for width calculation)
        var rawRows = new List<string[]>();
        var i = start;

        while (i < lines.Length && IsTableRow(lines[i]))
        {
            if (TableSeparatorPattern().IsMatch(lines[i].TrimEnd()))
            {
                i++;
                continue;
            }

            rawRows.Add(ParseTableCells(lines[i]));
            i++;
        }

        if (rawRows.Count == 0) return i - 1;

        // Compute column widths from plain text
        var colCount = rawRows.Max(r => r.Length);
        var widths = new int[colCount];
        foreach (var row in rawRows)
            for (var c = 0; c < row.Length; c++)
                widths[c] = Math.Max(widths[c], row[c].Length);

        // Render top border
        var topSb = new StringBuilder();
        topSb.Append($"{TableBorder}┌");
        for (var c = 0; c < colCount; c++)
        {
            topSb.Append(new string('─', widths[c] + 2));
            topSb.Append(c < colCount - 1 ? "┬" : "┐");
        }
        topSb.Append(Reset);
        result.Add(topSb.ToString());

        // Render header
        var headerRow = rawRows[0];
        var headerSb = new StringBuilder();
        headerSb.Append($"{TableBorder}│{Reset} ");
        for (var c = 0; c < colCount; c++)
        {
            var cell = c < headerRow.Length ? headerRow[c] : "";
            headerSb.Append($"{Bold}{cell.PadRight(widths[c])}{Reset}");
            if (c < colCount - 1)
                headerSb.Append($" {TableBorder}│{Reset} ");
        }
        headerSb.Append($" {TableBorder}│{Reset}");
        result.Add(headerSb.ToString());

        // Render separator line
        var sepSb = new StringBuilder();
        sepSb.Append($"{TableBorder}├");
        for (var c = 0; c < colCount; c++)
        {
            sepSb.Append(new string('─', widths[c] + 2));
            sepSb.Append(c < colCount - 1 ? "┼" : "┤");
        }
        sepSb.Append(Reset);
        result.Add(sepSb.ToString());

        // Render data rows with inline formatting
        for (var r = 1; r < rawRows.Count; r++)
        {
            var row = rawRows[r];
            var sb = new StringBuilder();
            sb.Append($"{TableBorder}│{Reset} ");
            for (var c = 0; c < colCount; c++)
            {
                var plainCell = c < row.Length ? row[c] : "";
                var formatted = FormatInline(plainCell);
                sb.Append(PadRightVisible(formatted, widths[c]));
                if (c < colCount - 1)
                    sb.Append($" {TableBorder}│{Reset} ");
            }
            sb.Append($" {TableBorder}│{Reset}");
            result.Add(sb.ToString());
        }

        // Render bottom border
        var botSb = new StringBuilder();
        botSb.Append($"{TableBorder}└");
        for (var c = 0; c < colCount; c++)
        {
            botSb.Append(new string('─', widths[c] + 2));
            botSb.Append(c < colCount - 1 ? "┴" : "┘");
        }
        botSb.Append(Reset);
        result.Add(botSb.ToString());

        return i - 1;
    }
}
