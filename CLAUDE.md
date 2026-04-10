# CLAUDE.md

## Commands

```bash
dotnet build                                          # Build
dotnet test                                           # Test
dotnet clean                                          # Clean
cd Jitzu.Shell && dotnet run -- ../Tests/script.jz    # Run script
cd Jitzu.Shell && dotnet run -- -d ../Tests/script.jz # Run with debug
```

## Structure

- `Jitzu.Core/` - Lexer, Parser, AST, bytecode, compilation, runtime
- `Jitzu.Shell/` - CLI entry point, interpreter, shell REPL
- `Jitzu.Tests/` - Unit tests
- `Tests/` - Integration test scripts (`.jz`)
- `site/` - Docs website
- `extensions/` - VS Code extension

**Solution**: `Jitzu.sln` (.NET 10.0)

## Execution Pipeline

```
Source (.jz) → Lexer → Parser → AST
  → ProgramBuilder → SemanticAnalyser (2-pass) → AstTransformer
  → ByteCodeCompiler → ByteCodeInterpreter
```

Orchestration: `Jitzu.Shell/Program.cs`

## Key Files

| Area | Path |
|------|------|
| Front-end | `Jitzu.Core/Lexer.cs`, `Parser.cs`, `Language/Expressions.cs`, `Language/Token.cs` |
| Compilation | `Jitzu.Core/Runtime/Compilation/` — `ProgramBuilder.cs`, `SemanticAnalyser.cs`, `AstTransformer.cs`, `ByteCodeCompiler.cs` |
| Runtime | `Jitzu.Core/ByteCodeInterpreter.cs`, `Runtime/ProgramStack.cs`, `Runtime/RuntimeProgram.cs`, `Runtime/OpCode.cs` |
| Types | `Jitzu.Core/Runtime/Compilation/UserTypeEmitter.cs`, `Runtime/TypeRegistry.cs` |

## Testing

- Framework: **TUnit** (not xUnit/NUnit) — uses `[Test]` attribute, `Shouldly` assertions
- Filter: `dotnet test --treenode-filter "/ClassName/**"` (not `--filter`)
- Tests run in **parallel by default** — avoid `Environment.CurrentDirectory` in tests; use absolute paths
- Shell commands: `ThemeConfig.CreateDefault()` + `new CommandContext(new ShellSession(), theme)` for lightweight test setup (no filesystem I/O)
- `Jitzu.Shell` has `InternalsVisibleTo` for `Jitzu.Tests`
