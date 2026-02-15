using System.Text;

namespace Jitzu.Shell.Core.Commands;

/// <summary>
/// Compares two files and shows differences.
/// </summary>
public class DiffCommand : CommandBase
{
    public DiffCommand(CommandContext context) : base(context) { }

    public override async Task<ShellResult> ExecuteAsync(ReadOnlyMemory<string> args)
    {
        if (args.Length < 2)
            return new ShellResult(ResultType.Error, "", new Exception("Usage: diff <file1> <file2>"));

        try
        {
            var path1 = ExpandPath(args.Span[0]);
            var path2 = ExpandPath(args.Span[1]);

            if (!File.Exists(path1))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {args.Span[0]}"));
            if (!File.Exists(path2))
                return new ShellResult(ResultType.Error, "", new Exception($"File not found: {args.Span[1]}"));

            var lines1 = await File.ReadAllLinesAsync(path1);
            var lines2 = await File.ReadAllLinesAsync(path2);

            var sb = new StringBuilder();
            var addColor = Theme["git.staged"];   // green
            var delColor = Theme["error"];         // red
            var hunkColor = Theme["ls.code"];      // cyan
            var dim = ThemeConfig.Dim;
            var reset = ThemeConfig.Reset;

            sb.AppendLine($"{dim}--- {args.Span[0]}{reset}");
            sb.AppendLine($"{dim}+++ {args.Span[1]}{reset}");

            // Simple LCS-based diff
            var lcs = ComputeLcs(lines1, lines2);
            var i = 0;
            var j = 0;
            var k = 0;
            var hasDifferences = false;

            while (i < lines1.Length || j < lines2.Length)
            {
                if (k < lcs.Length && i < lines1.Length && j < lines2.Length && lines1[i] == lcs[k] && lines2[j] == lcs[k])
                {
                    // Common line — skip (context-less diff for brevity)
                    i++; j++; k++;
                }
                else if (k < lcs.Length && j < lines2.Length && (i >= lines1.Length || lines1[i] != lcs[k]))
                {
                    // Line only in file1 (deleted) or added in file2
                    if (i < lines1.Length && (k >= lcs.Length || lines1[i] != lcs[k]))
                    {
                        sb.AppendLine($"{delColor}-{lines1[i]}{reset}");
                        i++;
                        hasDifferences = true;
                    }
                    else
                    {
                        sb.AppendLine($"{addColor}+{lines2[j]}{reset}");
                        j++;
                        hasDifferences = true;
                    }
                }
                else if (i < lines1.Length)
                {
                    sb.AppendLine($"{delColor}-{lines1[i]}{reset}");
                    i++;
                    hasDifferences = true;
                }
                else if (j < lines2.Length)
                {
                    sb.AppendLine($"{addColor}+{lines2[j]}{reset}");
                    j++;
                    hasDifferences = true;
                }
            }

            if (!hasDifferences)
                return new ShellResult(ResultType.OsCommand, "Files are identical.", null);

            return new ShellResult(ResultType.OsCommand, sb.ToString().TrimEnd(), null);
        }
        catch (Exception ex)
        {
            return new ShellResult(ResultType.Error, "", ex);
        }
    }

    private static string[] ComputeLcs(string[] a, string[] b)
    {
        var m = a.Length;
        var n = b.Length;
        var dp = new int[m + 1, n + 1];

        for (var i = 1; i <= m; i++)
        {
            for (var j = 1; j <= n; j++)
            {
                dp[i, j] = a[i - 1] == b[j - 1]
                    ? dp[i - 1, j - 1] + 1
                    : Math.Max(dp[i - 1, j], dp[i, j - 1]);
            }
        }

        var lcs = new List<string>();
        int x = m, y = n;
        while (x > 0 && y > 0)
        {
            if (a[x - 1] == b[y - 1])
            {
                lcs.Add(a[x - 1]);
                x--; y--;
            }
            else if (dp[x - 1, y] > dp[x, y - 1])
                x--;
            else
                y--;
        }

        lcs.Reverse();
        return lcs.ToArray();
    }
}
