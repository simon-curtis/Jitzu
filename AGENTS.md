# AGENTS.md - Guidance for AI Agents

This file provides guidance for AI agents working on the Jitzu project.

## Shell Feature Testing

When adding new features to the shell (`Jitzu.Shell`), always write tests using the shell test harness.

### Test Harness Location
`Jitzu.Tests/ShellTestHarness.cs`

### Writing Tests

1. **Add tests to:** `Jitzu.Tests/ShellTests.cs`

2. **Test harness API:**
```csharp
// Spawn and start shell
await using var harness = new ShellTestHarness();
await harness.StartAsync(GetShellPath());

// Send command and wait for output
var output = await harness.SendCommandAsync("your-command");

// Fire-and-forget (for commands that don't produce output)
await harness.SendCommandAndWaitAsync("your-command");

// Get all captured output (includes ANSI codes - use AnsiStripper.Strip() to clean)
var fullOutput = harness.GetAllOutput();

// Check if shell exited
var hasExited = harness.HasExited;

// Cleanup
await harness.DisposeAsync();
```

3. **GetShellPath helper:**
```csharp
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
```

4. **Example test pattern:**
```csharp
[Test]
public async Task YourFeature_NameOfTest()
{
    // Arrange
    await using var harness = new ShellTestHarness();
    await harness.StartAsync(GetShellPath());
    
    await Task.Delay(500); // Give shell time to initialize
    
    // Act
    var output = await harness.SendCommandAsync("your-command args");
    
    // Assert
    output.ShouldContain("expected-output");
    
    // Cleanup
    await harness.DisposeAsync();
}
```

5. **Important notes:**
   - Always include `await Task.Delay(500)` after `StartAsync` to allow shell initialization
   - Use `harness.SendCommandAndWaitAsync` for commands that don't produce output
   - Use `AnsiStripper.Strip()` to remove ANSI color codes from output if needed
   - Tests are async and should use `IAsyncDisposable` for proper cleanup

### Running Tests

```bash
# Run all tests
dotnet test

# Run only shell tests
dotnet test --filter "ShellTests"
```

### Red/Green Workflow

1. **Red** - Write a failing test first
2. **Green** - Implement the feature to make test pass
3. **Refactor** - Clean up code while keeping tests passing

Always follow this cycle: write test → verify it fails → implement feature → verify it passes.
