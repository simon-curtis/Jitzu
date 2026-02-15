using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;

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

    public async Task StartAsync(string shellPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = shellPath,
            WorkingDirectory = WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = Encoding.UTF8,
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

        await Task.Delay(500);
    }

    public async Task<string> SendCommandAsync(string command, int timeoutMs = 5000)
    {
        if (_input == null || _process == null || _process.HasExited)
            throw new InvalidOperationException("Shell is not running");

        var outputBefore = _output.Length;
        
        _input.WriteLine(command);
        await _input.FlushAsync();

        var sw = Stopwatch.StartNew();
        
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(100);
            
            lock (_lock)
            {
                var output = _output.ToString();
                if (output.Length > outputBefore)
                {
                    var newOutput = output[outputBefore..];
                    var stripped = AnsiStripper.Strip(newOutput);
                    var lines = stripped.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    
                    if (lines.Length >= 2)
                    {
                        return stripped;
                    }
                }
            }
            
            if (_process.HasExited)
                break;
        }

        lock (_lock)
        {
            var allOutput = _output.ToString();
            return allOutput.Length > outputBefore ? AnsiStripper.Strip(allOutput[outputBefore..]) : "";
        }
    }

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

    public async Task<(string stdout, string stderr)> SendCommandRawAsync(string command, int timeoutMs = 5000)
    {
        if (_input == null || _process == null || _process.HasExited)
            throw new InvalidOperationException("Shell is not running");

        var outputBefore = _output.Length;
        
        _input.WriteLine(command);
        await _input.FlushAsync();

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(100);
            
            if (_process.HasExited)
                break;
        }

        lock (_lock)
        {
            var allOutput = _output.ToString();
            var newOutput = allOutput.Length > outputBefore ? allOutput[outputBefore..] : "";
            return (newOutput, "");
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
                _input?.WriteLine("exit");
                await _input?.FlushAsync()!;
                
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

public static class ShellHarnessExtensions
{
    public static async Task<ShellTestHarness> StartShellAsync(this ShellTestHarness harness, string shellPath)
    {
        await harness.StartAsync(shellPath);
        return harness;
    }

    public static async Task ShouldContainAsync(this ShellTestHarness harness, string command, string expected, int timeoutMs = 5000)
    {
        var output = await harness.SendCommandAsync(command, timeoutMs);
        output.ShouldContain(expected);
    }

    public static async Task ShouldNotContainAsync(this ShellTestHarness harness, string command, string notExpected, int timeoutMs = 5000)
    {
        var output = await harness.SendCommandAsync(command, timeoutMs);
        output.ShouldNotContain(notExpected);
    }
}
