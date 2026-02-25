using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Jitzu.Tests;

public static class AnsiStripper
{
    private static readonly Regex AnsiRegex = new(@"\x1B\[[0-9;]*[a-zA-Z]", RegexOptions.Compiled);
    
    public static string Strip(string input)
    {
        return AnsiRegex.Replace(input, "");
    }
}

public class ShellTestHarness : IAsyncDisposable
{
    public static string GetShellPath()
    {
        var exe = OperatingSystem.IsWindows() ? "jz.exe" : "jz";

        var debugPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Jitzu.Shell", "bin", "Debug", "net10.0", exe);
        if (File.Exists(debugPath))
            return debugPath;

        var releasePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Jitzu.Shell", "bin", "Release", "net10.0", exe);
        if (File.Exists(releasePath))
            return releasePath;

        return "jz";
    }

    private Process? _process;
    private StreamWriter? _input;
    private readonly StringBuilder _output = new();
    private readonly object _lock = new();
    private bool _disposed;

    public string WorkingDirectory { get; }

    public ShellTestHarness(string? workingDirectory = null)
    {
        WorkingDirectory = workingDirectory ?? Path.GetTempPath();
    }

    public async Task StartAsync(string shellPath, string arguments = "--no-persist --no-splash")
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = shellPath,
            Arguments = arguments,
            WorkingDirectory = WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        _process = new Process { StartInfo = startInfo };
        _process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (_lock)
                {
                    _output.AppendLine(e.Data);
                }
            }
        };
        _process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                lock (_lock)
                {
                    _output.AppendLine($"[ERR] {e.Data}");
                }
            }
        };

        _process.Start();
        _input = _process.StandardInput;
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        // Wait for shell readiness using a sentinel probe instead of a fixed delay
        var readySentinel = $"__READY_{Guid.NewGuid():N}__";
        _input.WriteLine($"echo {readySentinel}");
        await _input.FlushAsync();

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < 5000)
        {
            await Task.Delay(50);
            lock (_lock)
            {
                if (_output.ToString().Contains(readySentinel))
                    break;
            }

            if (_process.HasExited)
                break;
        }

        // Clear startup output so tests start with a clean baseline
        lock (_lock) { _output.Clear(); }
    }

    /// <summary>
    /// Sends a command and reliably captures its output using a sentinel marker.
    /// Sends a follow-up echo command on a separate line and polls until that sentinel
    /// appears, guaranteeing all prior output has been flushed.
    /// </summary>
    public async Task<string> SendCommandAsync(string command, int timeoutMs = 5000)
    {
        if (_input == null || _process == null || _process.HasExited)
            throw new InvalidOperationException("Shell is not running");

        var sentinel = $"__SENTINEL_{Guid.NewGuid():N}__";

        // Snapshot the current output length before sending, so we only look at new output
        int baseline;
        lock (_lock) { baseline = _output.Length; }

        // Send the command and sentinel as separate lines so `;` semantics don't interfere
        _input.WriteLine(command);
        _input.WriteLine($"echo {sentinel}");
        await _input.FlushAsync();

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(50);

            string newOutput;
            lock (_lock) { newOutput = _output.ToString()[baseline..]; }

            if (newOutput.Contains(sentinel))
            {
                var stripped = AnsiStripper.Strip(newOutput);
                var sentinelIdx = stripped.IndexOf(sentinel, StringComparison.Ordinal);
                return stripped[..sentinelIdx].Trim();
            }

            if (_process.HasExited)
                break;
        }

        lock (_lock) { return AnsiStripper.Strip(_output.ToString()[baseline..]).Trim(); }
    }

    /// <summary>
    /// Sends a command without waiting for output. Use when you don't need to capture the result.
    /// </summary>
    public async Task SendCommandAndWaitAsync(string command, int waitMs = 300)
    {
        if (_input == null || _process == null || _process.HasExited)
            throw new InvalidOperationException("Shell is not running");

        _input.WriteLine(command);
        await _input.FlushAsync();
        await Task.Delay(waitMs);
    }

    public string GetAllOutput()
    {
        lock (_lock)
        {
            return _output.ToString();
        }
    }

    public void ClearOutput()
    {
        lock (_lock)
        {
            _output.Clear();
        }
    }

    public bool HasExited => _process?.HasExited ?? true;

    public int? ExitCode => _process?.ExitCode;

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_process != null && !_process.HasExited)
        {
            try
            {
                if (_input is not null)
                {
                    _input.WriteLine("exit");
                    await _input.FlushAsync();
                }
                
                if (!_process.WaitForExit(2000))
                {
                    _process.Kill(true);
                }
            }
            catch { }
        }

        _input?.Dispose();
        _process?.Dispose();
        _input = null;
        _process = null;
    }
}
