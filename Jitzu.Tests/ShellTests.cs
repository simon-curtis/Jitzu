using System.Diagnostics;
using System.Text;
using Shouldly;

namespace Jitzu.Tests;

public class ShellTests
{
    [Test]
    public async Task Shell_StartsSuccessfully()
    {
        await using var harness = new ShellTestHarness();
        await harness.StartAsync(ShellTestHarness.GetShellPath());

        harness.HasExited.ShouldBeFalse();
    }

    [Test]
    public async Task PwdCommand_ReturnsCurrentDirectory()
    {
        await using var harness = new ShellTestHarness(Path.GetTempPath());
        await harness.StartAsync(ShellTestHarness.GetShellPath());

        var output = await harness.SendCommandAsync("pwd");

        output.ShouldContain(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar));
    }

    [Test]
    public async Task CdCommand_ChangesDirectory()
    {
        await using var harness = new ShellTestHarness(Path.GetTempPath());
        await harness.StartAsync(ShellTestHarness.GetShellPath());

        await harness.SendCommandAndWaitAsync("cd /");
        var output = await harness.SendCommandAsync("pwd");

        // On Windows, "cd /" goes to the drive root (e.g. "D:\"), on Unix it's "/"
        output.ShouldContain(Path.GetPathRoot(Environment.CurrentDirectory)!.TrimEnd(Path.DirectorySeparatorChar));
    }

    [Test]
    public async Task ExitCommand_ExitsShell()
    {
        await using var harness = new ShellTestHarness();
        await harness.StartAsync(ShellTestHarness.GetShellPath());

        await harness.SendCommandAndWaitAsync("exit", waitMs: 0);

        var sw = Stopwatch.StartNew();
        while (!harness.HasExited && sw.ElapsedMilliseconds < 3000)
            await Task.Delay(50);

        harness.HasExited.ShouldBeTrue();
    }

    [Test]
    public async Task MultipleCommands_ChainedWithSemicolon()
    {
        await using var harness = new ShellTestHarness();
        await harness.StartAsync(ShellTestHarness.GetShellPath());

        // Semicolon-chained commands only display the last command's output
        var output = await harness.SendCommandAsync("echo a; echo b; echo c");

        output.ShouldContain("c");
    }

    [Test]
    public async Task PipedInput_ExecutesMultipleCommands()
    {
        var (output, exitCode) = await RunPipedAsync("echo hello\necho world\n");

        exitCode.ShouldBe(0);
        output.ShouldContain("hello");
        output.ShouldContain("world");
    }

    [Test]
    public async Task PipedInput_EmptyInput_ExitsCleanly()
    {
        var (_, exitCode) = await RunPipedAsync("");

        exitCode.ShouldBe(0);
    }

    [Test]
    public async Task PipedInput_ExitCommand_StopsProcessing()
    {
        var (output, exitCode) = await RunPipedAsync("echo before\nexit\necho after\n");

        exitCode.ShouldBe(0);
        output.ShouldContain("before");
        output.ShouldNotContain("after");
    }

    private static async Task<(string output, int exitCode)> RunPipedAsync(string stdin)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ShellTestHarness.GetShellPath(),
            Arguments = "--no-persist --no-splash",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        using var process = Process.Start(startInfo)!;

        await process.StandardInput.WriteAsync(stdin);
        process.StandardInput.Close();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = await outputTask;
        var error = await errorTask;

        if (!string.IsNullOrWhiteSpace(error))
            throw new Exception($"Shell error: {error}");

        return (output, process.ExitCode);
    }
}
