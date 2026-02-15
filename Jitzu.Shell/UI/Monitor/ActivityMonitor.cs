using System.Diagnostics;
using System.Text;

namespace Jitzu.Shell.UI.Monitor;

/// <summary>
/// Full-screen live-updating activity monitor TUI.
/// Shows CPU/RAM sparkline graphs, disk usage bars, and a navigable process tree.
/// Styled with bordered panels, gradient sparklines, and color-coded metrics.
/// </summary>
internal sealed class ActivityMonitor
{
    private readonly ThemeConfig _theme;
    private readonly ScreenBuffer _screen = new();
    private readonly SystemMetricsCollector _metrics = new();
    private readonly ProcessTreeBuilder _treeBuilder = new();

    private List<ProcessRow> _processes = [];
    private List<ProcessRow> _filteredProcesses = [];
    private int _selectedIndex;
    private int _scrollOffset;
    private bool _shellChildrenOnly;
    private bool _searchMode;
    private string _searchQuery = "";
    private DateTime _lastKPress = DateTime.MinValue;
    private int _lastListHeight = 10;
    private int _lastWidth;
    private int _lastHeight;

    // ── Box drawing (rounded corners) ──
    private const char BoxTL = '\u256d'; // ╭
    private const char BoxTR = '\u256e'; // ╮
    private const char BoxBL = '\u2570'; // ╰
    private const char BoxBR = '\u256f'; // ╯
    private const char BoxV  = '\u2502'; // │
    private const char BoxH  = '\u2500'; // ─

    // ── Gradient stops for sparklines (green → yellow-green → yellow → orange → red) ──
    private static readonly string[] SparkGradient =
    [
        "\e[38;2;80;200;120m",   // green
        "\e[38;2;160;210;80m",   // yellow-green
        "\e[38;2;230;210;60m",   // yellow
        "\e[38;2;230;150;50m",   // orange
        "\e[38;2;220;70;60m",    // red
    ];

    // ── Panel chrome ──
    private const string BorderColor = "\e[38;2;80;90;120m";
    private const string TitleColor  = "\e[38;2;180;200;240m";
    private const string LabelDim    = "\e[38;2;120;130;150m";

    // ── CPU / RAM / Net accent colors ──
    private const string CpuAccent   = "\e[38;2;80;200;120m";
    private const string RamAccent   = "\e[38;2;100;160;230m";
    private const string NetAccent   = "\e[38;2;200;160;240m";
    private const string NetSendColor = "\e[38;2;230;150;50m";
    private const string NetRecvColor = "\e[38;2;100;200;230m";

    // ── Disk gauge colors ──
    private const string DiskLow     = "\e[38;2;80;200;120m";
    private const string DiskMid     = "\e[38;2;230;210;60m";
    private const string DiskHigh    = "\e[38;2;220;70;60m";
    private const string DiskEmpty   = "\e[38;2;50;50;55m";
    private const string GraphBg     = "\e[48;2;30;33;42m";

    // ── Process CPU% colors ──
    private const string ProcCpuLow  = "\e[38;2;80;200;120m";
    private const string ProcCpuMed  = "\e[38;2;230;210;60m";
    private const string ProcCpuHigh = "\e[38;2;220;70;60m";

    // ── Per-core bar pastel gradient (teal → sky → lavender → peach → rose) ──
    private static readonly string[] CoreGradient =
    [
        "\e[38;2;120;210;180m",  // soft teal
        "\e[38;2;130;195;220m",  // sky blue
        "\e[38;2;170;170;220m",  // lavender
        "\e[38;2;220;170;170m",  // peach
        "\e[38;2;210;120;130m",  // rose
    ];
    private const string CoreBarEmpty = "\e[38;2;55;58;68m";

    // ── Row tinting ──
    private const string RowTintB    = "\e[48;2;30;32;38m";

    // ── Header / Status ──
    private const string HeaderBg    = "\e[48;2;35;38;55m";
    private const string StatusKeyBg = "\e[48;2;60;65;90m";
    private const string StatusKeyFg = "\e[38;2;200;210;240m";
    private const string StatusDescFg = "\e[38;2;120;125;145m";

    // ── Selection ──
    private const string SelectedBg  = "\e[48;2;38;79;120m";

    // ── Column separator ──
    private const string ColSep      = "\e[38;2;55;58;70m";

    // ── General ──
    private const string Dim   = "\e[2m";
    private const string Bold  = "\e[1m";
    private const string Reset = "\e[0m";

    // Panel height constants
    private const int CpuGraphRows = 4;
    private const int RamGraphRows = 2;
    private const int NetGraphRows = 2;
    private const int CpuPanelHeight = CpuGraphRows + 2;
    private const int RamPanelHeight = RamGraphRows + 2;

    public ActivityMonitor(ThemeConfig theme)
    {
        _theme = theme;
    }

    public async Task RunAsync()
    {
        _metrics.Sample();
        await Task.Delay(200);

        try
        {
            Console.Write("\e[?1049h"); // switch to alternate screen buffer
            Console.CursorVisible = false;
            Console.Write("\e[2J\e[H");

            while (true)
            {
                var snapshot = _metrics.Sample();
                _processes = _treeBuilder.BuildTree(_shellChildrenOnly, Environment.ProcessId);
                ApplyFilter();

                RenderFrame(snapshot);

                for (var tick = 0; tick < 20; tick++)
                {
                    // Re-render on terminal resize
                    if (Console.WindowWidth != _lastWidth || Console.WindowHeight != _lastHeight)
                    {
                        Console.Write("\e[2J\e[H");
                        RenderFrame(snapshot);
                    }

                    while (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (HandleKey(key))
                            return;
                        RenderFrame(snapshot);
                    }
                    await Task.Delay(50);
                }
            }
        }
        finally
        {
            _metrics.Dispose();
            _screen.ShowCursor();
            Console.CursorVisible = true;
            Console.Write("\e[?1049l"); // restore main screen buffer
        }
    }

    /// <returns>true if quit requested</returns>
    private bool HandleKey(ConsoleKeyInfo key)
    {
        if (_searchMode)
        {
            if (key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Enter)
            {
                _searchMode = false;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (_searchQuery.Length > 0)
                {
                    _searchQuery = _searchQuery[..^1];
                    ApplyFilter();
                    _selectedIndex = 0;
                    _scrollOffset = 0;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                _searchQuery += key.KeyChar;
                ApplyFilter();
                _selectedIndex = 0;
                _scrollOffset = 0;
            }
            return false;
        }

        switch (key.Key)
        {
            case ConsoleKey.Q:
                return true;

            case ConsoleKey.Escape:
                if (_searchQuery.Length > 0)
                {
                    _searchQuery = "";
                    ApplyFilter();
                    _selectedIndex = 0;
                    _scrollOffset = 0;
                    break;
                }
                return true;

            case ConsoleKey.UpArrow:
                if (_selectedIndex > 0) _selectedIndex--;
                EnsureVisible();
                break;

            case ConsoleKey.DownArrow:
                if (_selectedIndex < _filteredProcesses.Count - 1) _selectedIndex++;
                EnsureVisible();
                break;

            case ConsoleKey.PageUp:
                _selectedIndex = Math.Max(0, _selectedIndex - PageSize());
                EnsureVisible();
                break;

            case ConsoleKey.PageDown:
                _selectedIndex = Math.Min(_filteredProcesses.Count - 1, _selectedIndex + PageSize());
                EnsureVisible();
                break;

            case ConsoleKey.Home:
                _selectedIndex = 0;
                _scrollOffset = 0;
                break;

            case ConsoleKey.End:
                _selectedIndex = Math.Max(0, _filteredProcesses.Count - 1);
                EnsureVisible();
                break;

            case ConsoleKey.T:
                _shellChildrenOnly = !_shellChildrenOnly;
                _selectedIndex = 0;
                _scrollOffset = 0;
                break;

            case ConsoleKey.K:
                var now = DateTime.UtcNow;
                if ((now - _lastKPress).TotalMilliseconds < 400)
                {
                    KillSelected();
                    _lastKPress = DateTime.MinValue;
                }
                else
                {
                    _lastKPress = now;
                }
                break;

            case ConsoleKey.Oem2 when key.KeyChar == '/':
            case ConsoleKey.Divide:
                _searchMode = true;
                _searchQuery = "";
                break;
        }

        return false;
    }

    private void KillSelected()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _filteredProcesses.Count) return;
        var pid = _filteredProcesses[_selectedIndex].Pid;
        try
        {
            var proc = Process.GetProcessById(pid);
            proc.Kill();
            proc.Dispose();
        }
        catch { }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrEmpty(_searchQuery))
        {
            _filteredProcesses = _processes;
        }
        else
        {
            var query = _searchQuery;
            var portQuery = query.StartsWith(':') ? query[1..] : query;
            var isPortSearch = int.TryParse(portQuery, out var port);

            _filteredProcesses = _processes
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || (isPortSearch && p.Ports.Length > 0 && p.Ports.Contains(port)))
                .ToList();
        }
    }

    private void EnsureVisible()
    {
        var pageSize = PageSize();
        if (_selectedIndex < _scrollOffset)
            _scrollOffset = _selectedIndex;
        else if (_selectedIndex >= _scrollOffset + pageSize)
            _scrollOffset = _selectedIndex - pageSize + 1;
    }

    private int PageSize() => Math.Max(1, _lastListHeight);

    // ── Rendering ──────────────────────────────────────────────

    private void RenderFrame(MetricsSnapshot snapshot)
    {
        var w = Console.WindowWidth;
        var h = Console.WindowHeight;
        _lastWidth = w;
        _lastHeight = h;
        var wide = w >= 100;
        var rowsRendered = 0;

        _screen.Begin();

        // ── Header ──
        RenderHeader(w);
        rowsRendered++;

        // ── CPU & RAM panels ──
        if (wide)
            rowsRendered += RenderSideBySidePanels(snapshot, w);
        else
            rowsRendered += RenderStackedPanels(snapshot, w);

        // ── Disk panel ──
        rowsRendered += RenderDiskPanel(snapshot, w);

        // ── Process column headers ──
        RenderProcessHeader(w);
        rowsRendered++;

        // ── Process list ──
        var listHeight = Math.Max(1, h - rowsRendered - 1); // -1 for status bar
        _lastListHeight = listHeight;
        RenderProcessList(w, listHeight);
        rowsRendered += listHeight;

        // ── Fill remaining space ──
        while (rowsRendered < h - 1)
        {
            _screen.AppendEmptyLine(w);
            rowsRendered++;
        }

        // ── Status bar (no trailing newline to prevent scroll) ──
        RenderStatusBarFinal(w);

        _screen.Flush();
    }

    private void RenderHeader(int w)
    {
        var hostname = Environment.MachineName;
        var uptime = GetUptime();
        var time = DateTime.Now.ToString("HH:mm:ss");

        var rightText = $"\u25b2 up {uptime}  \u23f1 {time} ";
        var leftText = $" \u25c6 Activity Monitor  {hostname}";
        var gap = Math.Max(1, w - leftText.Length - rightText.Length);

        var header = $"{HeaderBg}{Bold} {CpuAccent}\u25c6{Reset}{HeaderBg}{Bold} Activity Monitor  {Reset}{HeaderBg}{LabelDim}{hostname}{Reset}{HeaderBg}{new string(' ', gap)}{LabelDim}{rightText}{Reset}";
        _screen.AppendLine(header, w);
    }

    private int RenderStackedPanels(MetricsSnapshot snapshot, int w)
    {
        var rows = 0;
        rows += RenderCpuPanel(snapshot, 0, w);
        rows += RenderRamPanel(snapshot, 0, w);
        rows += RenderNetPanel(snapshot, 0, w);
        return rows;
    }

    private int RenderSideBySidePanels(MetricsSnapshot snapshot, int w)
    {
        var halfW = w / 2;
        var rightW = w - halfW;
        var cpuLines = BuildCpuPanelLines(snapshot, halfW);
        var ramLines = BuildRamPanelLines(snapshot, rightW);
        var netLines = BuildNetPanelLines(snapshot, rightW);

        // Right side stacks RAM + Network
        var rightLines = new List<string>(ramLines.Count + netLines.Count);
        rightLines.AddRange(ramLines);
        rightLines.AddRange(netLines);

        var maxLines = Math.Max(cpuLines.Count, rightLines.Count);
        for (var i = 0; i < maxLines; i++)
        {
            var left = i < cpuLines.Count ? cpuLines[i] : "";
            var right = i < rightLines.Count ? rightLines[i] : "";

            // Pad left panel to exactly halfW visible chars
            var leftVisible = ScreenBuffer.VisibleLength(left);
            if (leftVisible < halfW)
                left += new string(' ', halfW - leftVisible);

            var combined = left + right;
            _screen.AppendLine(combined, w);
        }

        return maxLines;
    }

    // ── CPU Panel ──

    private int RenderCpuPanel(MetricsSnapshot snapshot, int leftMargin, int panelWidth)
    {
        var lines = BuildCpuPanelLines(snapshot, panelWidth);
        foreach (var line in lines)
            _screen.AppendLine(line, panelWidth);
        return lines.Count;
    }

    private List<string> BuildCpuPanelLines(MetricsSnapshot snapshot, int panelWidth)
    {
        var innerW = Math.Max(10, panelWidth - 2); // 1 padding each side
        var graphW = Math.Max(6, innerW - 14); // space for stats on right

        var lines = new List<string>();

        // Top border with title
        var coreCountLabel = snapshot.CoreCpuPercents.Length > 0 ? $" {snapshot.CoreCpuPercents.Length} cores" : "";
        lines.Add(PanelTopBorder($"CPU{coreCountLabel}", panelWidth));

        // Graph rows
        var graph = SparklineRenderer.RenderGradientGraph(_metrics.CpuHistory, graphW, CpuGraphRows, SparkGradient, Reset, GraphBg);
        foreach (var row in graph)
        {
            var visible = ScreenBuffer.VisibleLength(row);
            var pctLabel = $"{CpuAccent}{Bold}{snapshot.CpuPercent,5:F1}%{Reset}";
            // Only show percentage on first graph row
            var rightLabel = row == graph[0] ? pctLabel : "";
            var rightVisible = row == graph[0] ? 6 : 0;
            var gap = Math.Max(0, innerW - visible - rightVisible);
            lines.Add($" {row}{new string(' ', gap)}{rightLabel}{Reset}");
        }

        // Stats row
        var hist = _metrics.CpuHistory;
        var min = hist.Length > 0 ? hist.ToArray().Min() : 0;
        var avg = hist.Length > 0 ? hist.ToArray().Average() : 0;
        var max = hist.Length > 0 ? hist.ToArray().Max() : 0;
        var stats = $"{LabelDim}min:{Reset} {min,4:F0}%  {LabelDim}avg:{Reset} {avg,4:F0}%  {LabelDim}max:{Reset} {max,4:F0}%";
        var statsVisible = ScreenBuffer.VisibleLength(stats);
        var statsPad = Math.Max(0, innerW - statsVisible);
        lines.Add($" {stats}{new string(' ', statsPad)}{Reset}");

        // Per-core mini bars
        if (snapshot.CoreCpuPercents.Length > 0)
        {
            // Determine how many cores fit per row: each entry is "XX ████░░ NN%  " (~17 chars)
            var entryWidth = 18;
            var coresPerRow = Math.Max(1, innerW / entryWidth);
            var cores = snapshot.CoreCpuPercents;
            var barWidth = 8;

            for (var row_i = 0; row_i < cores.Length; row_i += coresPerRow)
            {
                var sb = new StringBuilder();
                var visibleLen = 0;
                var count = Math.Min(coresPerRow, cores.Length - row_i);

                for (var j = 0; j < count; j++)
                {
                    var coreIdx = row_i + j;
                    var pct = cores[coreIdx];

                    // Core label
                    var label = $"{coreIdx,2} ";
                    sb.Append(LabelDim);
                    sb.Append(label);
                    sb.Append(Reset);
                    visibleLen += label.Length;

                    // Pick bar color based on load level
                    var barColor = pct > 85 ? CoreGradient[4]
                                 : pct > 65 ? CoreGradient[3]
                                 : pct > 40 ? CoreGradient[2]
                                 : pct > 15 ? CoreGradient[1]
                                 : CoreGradient[0];

                    // Gradient bar — solid color per core
                    var filled = (int)(pct / 100.0 * barWidth);
                    var empty = barWidth - filled;
                    if (filled > 0)
                    {
                        sb.Append(barColor);
                        sb.Append('\u2588', filled); // █
                    }
                    if (empty > 0)
                    {
                        sb.Append(CoreBarEmpty);
                        sb.Append('\u2500', empty); // ─
                    }
                    sb.Append(Reset);
                    visibleLen += barWidth;

                    // Percentage
                    var pctStr = pct < 0.05 ? "  - " : $"{pct,3:F0}%";
                    var pctColor = pct > 70 ? CoreGradient[4] : pct > 30 ? CoreGradient[2] : LabelDim;
                    sb.Append(pctColor);
                    sb.Append(pctStr);
                    sb.Append(Reset);
                    visibleLen += pctStr.Length;

                    // Spacing between entries
                    if (j < count - 1)
                    {
                        sb.Append(' ');
                        visibleLen++;
                    }
                }

                var pad = Math.Max(0, innerW - visibleLen);
                lines.Add($" {sb}{new string(' ', pad)}{Reset}");
            }
        }

        // Bottom border
        lines.Add(PanelBottomBorder(panelWidth));

        return lines;
    }

    // ── RAM Panel ──

    private int RenderRamPanel(MetricsSnapshot snapshot, int leftMargin, int panelWidth)
    {
        var lines = BuildRamPanelLines(snapshot, panelWidth);
        foreach (var line in lines)
            _screen.AppendLine(line, panelWidth);
        return lines.Count;
    }

    private List<string> BuildRamPanelLines(MetricsSnapshot snapshot, int panelWidth)
    {
        var innerW = Math.Max(10, panelWidth - 2);
        var graphW = Math.Max(6, innerW - 14);

        var lines = new List<string>();

        // Top border with title
        lines.Add(PanelTopBorder("RAM", panelWidth));

        // Graph rows
        var graph = SparklineRenderer.RenderGradientGraph(_metrics.RamHistory, graphW, RamGraphRows, SparkGradient, Reset, GraphBg);
        foreach (var row in graph)
        {
            var visible = ScreenBuffer.VisibleLength(row);
            var pctLabel = $"{RamAccent}{Bold}{snapshot.Ram.UsedPercent,5:F1}%{Reset}";
            var rightLabel = row == graph[0] ? pctLabel : "";
            var rightVisible = row == graph[0] ? 6 : 0;
            var gap = Math.Max(0, innerW - visible - rightVisible);
            lines.Add($" {row}{new string(' ', gap)}{rightLabel}{Reset}");
        }

        // Stats row
        var usedStr = FormatBytes(snapshot.Ram.UsedBytes);
        var totalStr = FormatBytes(snapshot.Ram.TotalBytes);
        var stats = $"{LabelDim}used:{Reset} {usedStr}  {LabelDim}total:{Reset} {totalStr}";
        var statsVisible = ScreenBuffer.VisibleLength(stats);
        var statsPad = Math.Max(0, innerW - statsVisible);
        lines.Add($" {stats}{new string(' ', statsPad)}{Reset}");

        // Bottom border
        lines.Add(PanelBottomBorder(panelWidth));

        return lines;
    }

    // ── Network Panel ──

    private int RenderNetPanel(MetricsSnapshot snapshot, int leftMargin, int panelWidth)
    {
        var lines = BuildNetPanelLines(snapshot, panelWidth);
        foreach (var line in lines)
            _screen.AppendLine(line, panelWidth);
        return lines.Count;
    }

    private List<string> BuildNetPanelLines(MetricsSnapshot snapshot, int panelWidth)
    {
        var innerW = Math.Max(10, panelWidth - 2);
        var graphW = Math.Max(6, innerW - 14);

        var lines = new List<string>();

        // Top border with title
        lines.Add(PanelTopBorder("Network", panelWidth));

        // Graph rows — use recv history scaled to a dynamic max
        var recvHist = _metrics.NetRecvHistory;
        var sendHist = _metrics.NetSendHistory;

        // Find peak across both for scaling
        double peak = 1;
        for (var i = 0; i < recvHist.Length; i++)
            peak = Math.Max(peak, recvHist[i]);
        for (var i = 0; i < sendHist.Length; i++)
            peak = Math.Max(peak, sendHist[i]);

        // Scale to 0-100 for the sparkline renderer
        var recvScaled = new double[recvHist.Length];
        for (var i = 0; i < recvHist.Length; i++)
            recvScaled[i] = recvHist[i] / peak * 100;

        var recvGradient = new[] { NetRecvColor, NetRecvColor, NetRecvColor, NetRecvColor, NetRecvColor };
        var graph = SparklineRenderer.RenderGradientGraph(recvScaled, graphW, NetGraphRows, recvGradient, Reset, GraphBg);

        var peakLabel = $"{NetAccent}{FormatRate(peak)}{Reset}";
        var peakVisible = ScreenBuffer.VisibleLength(peakLabel);
        foreach (var row in graph)
        {
            var visible = ScreenBuffer.VisibleLength(row);
            var rightLabel = row == graph[0] ? peakLabel : "";
            var rightVisible = row == graph[0] ? peakVisible : 0;
            var gap = Math.Max(0, innerW - visible - rightVisible);
            lines.Add($" {row}{new string(' ', gap)}{rightLabel}{Reset}");
        }

        // Stats row — send / recv rates
        var sendRate = FormatRate(snapshot.Net.SendBytesPerSec);
        var recvRate = FormatRate(snapshot.Net.RecvBytesPerSec);
        var stats = $"{NetSendColor}\u25b2{Reset} {LabelDim}send:{Reset} {sendRate}  {NetRecvColor}\u25bc{Reset} {LabelDim}recv:{Reset} {recvRate}";
        var statsVisible = ScreenBuffer.VisibleLength(stats);
        var statsPad = Math.Max(0, innerW - statsVisible);
        lines.Add($" {stats}{new string(' ', statsPad)}{Reset}");

        // Bottom border
        lines.Add(PanelBottomBorder(panelWidth));

        return lines;
    }

    private static string FormatRate(double bytesPerSec)
    {
        return bytesPerSec switch
        {
            >= 1_000_000_000 => $"{bytesPerSec / 1_000_000_000:F1} GB/s",
            >= 1_000_000     => $"{bytesPerSec / 1_000_000:F1} MB/s",
            >= 1_000         => $"{bytesPerSec / 1_000:F1} KB/s",
            _                => $"{bytesPerSec:F0}  B/s"
        };
    }

    // ── Disk Panel ──

    private int RenderDiskPanel(MetricsSnapshot snapshot, int w)
    {
        var lines = new List<string>();
        var innerW = Math.Max(10, w - 2);

        lines.Add(PanelTopBorder("Disk", w));

        // Fixed-width right columns: "  44%  218.5 G / 500.0 G"
        // pct=5, used=8, slash=3, total=8 → 24 chars
        const int pctColW = 5;   // "  44%" or "   0%"
        const int sizeColW = 8;  // " 218.5 G" or "1907.7 G"
        const int rightFixedW = pctColW + 1 + sizeColW + 3 + sizeColW; // pct + space + used + " / " + total
        const int labelW = 4;    // "C:  "

        foreach (var disk in snapshot.Disks)
        {
            var pct = disk.TotalBytes > 0 ? (double)disk.UsedBytes / disk.TotalBytes : 0;
            var barWidth = Math.Max(4, innerW - labelW - rightFixedW - 3);

            var gauge = BuildDiskGauge(pct, barWidth);
            var diskColor = pct > 0.85 ? DiskHigh : pct > 0.60 ? DiskMid : DiskLow;
            var pctStr = $"{pct * 100,4:F0}%";
            var usedStr = FormatBytes(disk.UsedBytes).PadLeft(sizeColW);
            var totalStr = FormatBytes(disk.TotalBytes).PadLeft(sizeColW);
            var line = $"{LabelDim}{disk.Name,-4}{Reset}{gauge} {diskColor}{pctStr}{Reset} {LabelDim}{usedStr} / {totalStr}{Reset}";
            var lineVisible = ScreenBuffer.VisibleLength(line);
            var pad = Math.Max(0, innerW - lineVisible);
            lines.Add($" {line}{new string(' ', pad)}{Reset}");
        }

        if (snapshot.Disks.Length == 0)
        {
            var msg = $"{LabelDim}No drives detected{Reset}";
            var pad = Math.Max(0, innerW - ScreenBuffer.VisibleLength(msg));
            lines.Add($" {msg}{new string(' ', pad)}{Reset}");
        }

        lines.Add(PanelBottomBorder(w));

        foreach (var line in lines)
            _screen.AppendLine(line, w);

        return lines.Count;
    }

    private static string BuildDiskGauge(double pct, int barWidth)
    {
        var filled = (int)(pct * barWidth);
        var empty = barWidth - filled;

        var sb = new StringBuilder();
        sb.Append(GraphBg); // solid background like CPU/RAM graphs

        // Gradient fill: each character's color based on its position in the bar
        var lastColor = "";
        for (var i = 0; i < filled; i++)
        {
            var posPct = (double)i / barWidth;
            var color = posPct > 0.85 ? DiskHigh : posPct > 0.60 ? DiskMid : DiskLow;
            if (color != lastColor)
            {
                sb.Append(color);
                lastColor = color;
            }
            sb.Append('\u2588'); // █
        }

        if (empty > 0)
        {
            sb.Append(' ', empty);
        }

        sb.Append(Reset);
        return sb.ToString();
    }

    // ── Panel border helpers ──

    private static string PanelTopBorder(string title, int width)
    {
        //  Title ─────────
        var titlePart = $" {TitleColor}{title}{Reset}";
        var titleVisible = 1 + title.Length; // leading space + title text
        var lineLen = Math.Max(0, width - titleVisible - 1); // trailing space
        return $"{titlePart} {BorderColor}{new string(BoxH, lineLen)}{Reset}";
    }

    private static string PanelBottomBorder(int width)
    {
        return $" {BorderColor}{new string(BoxH, Math.Max(0, width - 2))}{Reset}";
    }

    // ── Process header & list ──

    private void RenderProcessHeader(int w)
    {
        var mode = _shellChildrenOnly ? "shell" : "all";
        var searchInfo = _searchMode ? $"  /{_searchQuery}\u2588" : (_searchQuery.Length > 0 ? $"  filter: {_searchQuery}" : "");
        var pidCol = "PID".PadRight(8);
        var cpuCol = "CPU%".PadLeft(7);
        var memCol = "MEM".PadLeft(9);
        var headerLine = $"{HeaderBg}{Bold}  {pidCol} {ColSep}{BoxV}{Reset}{HeaderBg}{Bold} {cpuCol} {ColSep}{BoxV}{Reset}{HeaderBg}{Bold} {memCol} {ColSep}{BoxV}{Reset}{HeaderBg}{Bold} NAME  [{mode}]{searchInfo}{Reset}";
        _screen.AppendLine(headerLine, w);
    }

    private void RenderProcessList(int w, int listHeight)
    {
        for (var i = _scrollOffset; i < _scrollOffset + listHeight; i++)
        {
            if (i < _filteredProcesses.Count)
            {
                var proc = _filteredProcesses[i];
                var isSelected = i == _selectedIndex;
                var isEvenRow = (i - _scrollOffset) % 2 == 1;

                var bg = isSelected ? SelectedBg : isEvenRow ? RowTintB : "";

                var pidStr = proc.Pid.ToString().PadRight(8);

                // Color-coded CPU%
                string cpuStr;
                if (proc.CpuPercent < 0.05)
                {
                    cpuStr = $"{LabelDim}   -   {Reset}";
                }
                else
                {
                    var cpuColor = proc.CpuPercent > 70 ? ProcCpuHigh : proc.CpuPercent > 30 ? ProcCpuMed : ProcCpuLow;
                    cpuStr = $"{cpuColor}{proc.CpuPercent,6:F1}%{Reset}";
                }

                var memStr = FormatBytes(proc.MemoryBytes).PadLeft(9);
                var sep = $"{ColSep}{BoxV}{Reset}";

                var portSuffix = proc.Ports.Length > 0
                    ? $"  {LabelDim}{string.Join(" ", proc.Ports.Select(p => $":{p}"))}{Reset}"
                    : "";
                var line = $"{bg}  {pidStr} {sep}{bg} {cpuStr}{bg} {sep}{bg} {memStr} {sep}{bg} {proc.TreePrefix}{proc.Name}{portSuffix}{Reset}";
                _screen.AppendLine(line, w);
            }
            else
            {
                _screen.AppendEmptyLine(w);
            }
        }
    }

    private void RenderStatusBarFinal(int w)
    {
        var sep = $" {ColSep}{BoxV}{Reset} ";
        string status;
        if (_searchMode)
        {
            status = $" {StatusKeyBg}{StatusKeyFg} esc {Reset} {StatusDescFg}confirm{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} enter {Reset} {StatusDescFg}confirm{Reset}"
                   + sep
                   + $"{StatusDescFg}type to filter{Reset}";
        }
        else if (_searchQuery.Length > 0)
        {
            status = $" {StatusKeyBg}{StatusKeyFg} q {Reset} {StatusDescFg}quit{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} \u2191\u2193 {Reset} {StatusDescFg}nav{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} kk {Reset} {StatusDescFg}kill{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} esc {Reset} {StatusDescFg}clear filter{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} / {Reset} {StatusDescFg}new search{Reset}";
        }
        else
        {
            status = $" {StatusKeyBg}{StatusKeyFg} q {Reset} {StatusDescFg}quit{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} \u2191\u2193 {Reset} {StatusDescFg}nav{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} / {Reset} {StatusDescFg}search/port{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} t {Reset} {StatusDescFg}toggle{Reset}"
                   + sep
                   + $"{StatusKeyBg}{StatusKeyFg} kk {Reset} {StatusDescFg}kill{Reset}";
        }
        // Use AppendRaw + manual padding — no trailing \n on the last row
        // to prevent the terminal from scrolling down each frame
        var visible = ScreenBuffer.VisibleLength(status);
        if (visible < w)
            status += new string(' ', w - visible);
        _screen.AppendRaw(status);
    }

    // ── Utilities ──

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1L << 30 => $"{bytes / (1024.0 * 1024 * 1024):F1} G",
            >= 1L << 20 => $"{bytes / (1024.0 * 1024):F1} M",
            >= 1L << 10 => $"{bytes / 1024.0:F1} K",
            _ => $"{bytes} B"
        };
    }

    private static string GetUptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        if (uptime.TotalDays >= 1)
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h";
        if (uptime.TotalHours >= 1)
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        return $"{(int)uptime.TotalMinutes}m";
    }
}
