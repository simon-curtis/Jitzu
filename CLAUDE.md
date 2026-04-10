# CLAUDE.md

## Commands

```bash
dotnet build                                          # Build
dotnet test                                           # Test
cd Jitzu.Shell && dotnet run -- ../Tests/script.jz    # Run script
cd Jitzu.Shell && dotnet run -- -d ../Tests/script.jz # Run with debug
```

## Pipeline

`Source (.jz) → Lexer → Parser → AST → ProgramBuilder → SemanticAnalyser (2-pass) → AstTransformer → ByteCodeCompiler → ByteCodeInterpreter`

## Testing

- **TUnit** (not xUnit/NUnit) — `[Test]` attribute, `Shouldly` assertions
- IMPORTANT: filter with `dotnet test --treenode-filter "/ClassName/**"` (not `--filter`)
- Tests run in parallel — use absolute paths, not `Environment.CurrentDirectory`
- Shell command tests: `ThemeConfig.CreateDefault()` + `new CommandContext(new ShellSession(), theme)`
- Shell integration tests: use `ShellTestHarness` in `Jitzu.Tests/ShellTestHarness.cs`
- `Jitzu.Shell` has `InternalsVisibleTo` for `Jitzu.Tests`
