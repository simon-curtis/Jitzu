using System.Text;

namespace Jitzu.Shell.UI.Monitor;

/// <summary>
/// Renders multi-row sparkline graphs using Unicode block characters (▁▂▃▄▅▆▇█).
/// Values are expected in 0–100 range.
/// </summary>
internal static class SparklineRenderer
{
    private static readonly char[] Blocks = [' ', '\u2581', '\u2582', '\u2583', '\u2584', '\u2585', '\u2586', '\u2587', '\u2588'];

    /// <summary>
    /// Renders a multi-row sparkline graph with a single color.
    /// </summary>
    public static string[] RenderGraph(ReadOnlySpan<double> values, int width, int height, string color, string reset)
    {
        var rows = new string[height];
        var bandSize = 100.0 / height;

        var start = values.Length > width ? values.Length - width : 0;
        var count = Math.Min(values.Length, width);
        var pad = width - count;

        for (var row = 0; row < height; row++)
        {
            var bandTop = 100.0 - row * bandSize;
            var bandBottom = bandTop - bandSize;

            var chars = new char[width];

            for (var i = 0; i < pad; i++)
                chars[i] = ' ';

            for (var i = 0; i < count; i++)
            {
                var val = Math.Clamp(values[start + i], 0, 100);

                if (val <= bandBottom)
                    chars[pad + i] = ' ';
                else if (val >= bandTop)
                    chars[pad + i] = Blocks[8];
                else
                {
                    var fraction = (val - bandBottom) / bandSize;
                    var idx = (int)(fraction * 8);
                    chars[pad + i] = Blocks[Math.Clamp(idx, 0, 8)];
                }
            }

            rows[row] = $"{color}{new string(chars)}{reset}";
        }

        return rows;
    }

    /// <summary>
    /// Renders a multi-row sparkline graph with gradient colors.
    /// Each column's color is selected based on the value at that position.
    /// </summary>
    /// <param name="values">History values (0-100)</param>
    /// <param name="width">Available character width</param>
    /// <param name="height">Number of rows for the graph</param>
    /// <param name="gradientStops">Array of ANSI color codes from low to high</param>
    /// <param name="reset">ANSI reset code</param>
    /// <param name="bgColor">Optional ANSI background color for the graph area</param>
    public static string[] RenderGradientGraph(ReadOnlySpan<double> values, int width, int height, string[] gradientStops, string reset, string? bgColor = null)
    {
        var rows = new string[height];
        var bandSize = 100.0 / height;

        var start = values.Length > width ? values.Length - width : 0;
        var count = Math.Min(values.Length, width);
        var pad = width - count;

        // Pre-compute per-column color index based on value
        var columnColors = new int[count];
        for (var i = 0; i < count; i++)
        {
            var val = Math.Clamp(values[start + i], 0, 100);
            var stopIndex = (int)(val / 100.0 * (gradientStops.Length - 1));
            columnColors[i] = Math.Clamp(stopIndex, 0, gradientStops.Length - 1);
        }

        for (var row = 0; row < height; row++)
        {
            var bandTop = 100.0 - row * bandSize;
            var bandBottom = bandTop - bandSize;

            var sb = new StringBuilder(width * 2);
            var lastColorIdx = -1;

            // Apply background color for the whole row
            if (bgColor != null)
                sb.Append(bgColor);

            // Left-pad with spaces
            if (pad > 0)
                sb.Append(' ', pad);

            for (var i = 0; i < count; i++)
            {
                var val = Math.Clamp(values[start + i], 0, 100);
                char ch;

                if (val <= bandBottom)
                    ch = ' ';
                else if (val >= bandTop)
                    ch = Blocks[8];
                else
                {
                    var fraction = (val - bandBottom) / bandSize;
                    var idx = (int)(fraction * 8);
                    ch = Blocks[Math.Clamp(idx, 0, 8)];
                }

                if (ch != ' ')
                {
                    var colorIdx = columnColors[i];
                    if (colorIdx != lastColorIdx)
                    {
                        sb.Append(gradientStops[colorIdx]);
                        lastColorIdx = colorIdx;
                    }
                }

                sb.Append(ch);
            }

            sb.Append(reset);
            rows[row] = sb.ToString();
        }

        return rows;
    }
}
