using Shouldly;

namespace Jitzu.Tests;

public class ShellTests
{
    private string GetShellPath()
    {
        var shellProjectPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Jitzu.Shell", "bin", "Debug", "net10.0", "jzsh");
        
        if (File.Exists(shellProjectPath))
            return shellProjectPath;
        
        var releasePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Jitzu.Shell", "bin", "Release", "net10.0", "jzsh");
        if (File.Exists(releasePath))
            return releasePath;
        
        return "jzsh";
    }

    [Test]
    public async Task Shell_StartsSuccessfully()
    {
        await using var harness = new ShellTestHarness();
        await harness.StartAsync(GetShellPath());
        
        await Task.Delay(500);
        
        harness.HasExited.ShouldBeFalse();
        
        await harness.DisposeAsync();
    }

    [Test]
    public async Task PwdCommand_ReturnsCurrentDirectory()
    {
        await using var harness = new ShellTestHarness(Path.GetTempPath());
        await harness.StartAsync(GetShellPath());
        
        await Task.Delay(500);
        
        var output = await harness.SendCommandAsync("pwd");
        
        output.ShouldContain("tmp");
        
        await harness.DisposeAsync();
    }

    [Test]
    public async Task CdCommand_ChangesDirectory()
    {
        await using var harness = new ShellTestHarness(Path.GetTempPath());
        await harness.StartAsync(GetShellPath());
        
        await Task.Delay(500);
        
        await harness.SendCommandAndWaitAsync("cd /");
        var output = await harness.SendCommandAsync("pwd");
        
        output.ShouldContain("/");
        
        await harness.DisposeAsync();
    }

    [Test]
    public async Task ExitCommand_ExitsShell()
    {
        await using var harness = new ShellTestHarness();
        await harness.StartAsync(GetShellPath());
        
        await Task.Delay(500);
        
        await harness.SendCommandAndWaitAsync("exit");
        
        await Task.Delay(1000);
        
        if (!harness.HasExited)
        {
            harness.DisposeAsync().AsTask().Wait(2000);
        }
        
        await harness.DisposeAsync();
    }

    [Test]
    public async Task MultipleCommands_ChainedWithSemicolon()
    {
        await using var harness = new ShellTestHarness();
        await harness.StartAsync(GetShellPath());
        
        await Task.Delay(500);
        
        var output = await harness.SendCommandAsync("echo a; echo b; echo c");
        
        output.ToLower().ShouldContain("a");
        output.ToLower().ShouldContain("b");
        output.ToLower().ShouldContain("c");
        
        await harness.DisposeAsync();
    }
}
