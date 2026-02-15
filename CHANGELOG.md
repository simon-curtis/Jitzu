# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-02-15

### Added
- **Interpreter (jz)**: Full scripting language interpreter
- **Shell (jzsh)**: Interactive REPL with command execution
- **Built-in Types**: Int, String, Bool, Double, Char, Date, Time, DateTime
- **Union Types**: Result<T, E>, Option<T>, Ok<T>, Err<E>, Some<T>, None<T>
- **NuGet Support**: Load and use .NET NuGet packages dynamically
- **User Types**: Define custom types with fields and methods using Reflection.Emit
- **Namespace Support**: File-based namespacing for user types, full CLR namespace preservation for NuGet
- **Shell Features**:
  - Command chaining (|, &&, ||)
  - I/O redirection (<, >, >>)
  - Glob expansion
  - Job control (background jobs, fg, bg)
  - Built-in commands (cd, ls, cat, echo, pwd, etc.)
  - Tab completion
  - Command history

### Language Features
- Functions with Result/Option return types
- Pattern matching with match expressions
- Safe indexing (Option<T> return)
- String interpolation with backticks
- Type inference
- Local variables with let binding

### Architecture
- Three-phase interpreter: Lexer → Parser → Semantic Analyser → Bytecode Compiler → VM
- Stack-based bytecode VM
- Two-pass semantic analysis for type resolution
- Reflection.Emit for dynamic user type creation

### Documentation
- Online documentation at https://jitzu.simoncurtis.dev
- Language guide, quick reference, and feature comparison docs

### Downloads
- Pre-built binaries for Linux x64, Windows x64, macOS x64, macOS ARM64
