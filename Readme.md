<p align="center">
  <a href="https://jitzu.simoncurtis.dev">
    <img src="https://jitzu.simoncurtis.dev/ninja-500.png?" alt="Logo" height=170>
  </a>
</p>

<h1 align="center">Jitzu</h1>

<div align="center">
  <a href="https://jitzu.simoncurtis.dev/docs">Documentation</a>
  <span>&nbsp;&nbsp;•&nbsp;&nbsp;</span>
  <a href="https://github.com/simon-curtis/jitzu/releases">Downloads</a>
  <span>&nbsp;&nbsp;•&nbsp;&nbsp;</span>
  <a href="https://github.com/simon-curtis/jitzu/issues/new">Issues</a>
  <br />
</div>

<br>
<br>

Jitzu is a lightweight, expressive scripting language that runs on the .NET runtime, designed for speed, flexibility, and simplicity. It can be:

- **Interpreted** – Run scripts instantly with the `jz` interpreter
- **Used as a Shell** – Execute commands interactively with `jzsh`
- **Extended with NuGet** – Import any .NET package directly
- **User-Defined Types** – Create custom types with fields and methods

## Installation

Download pre-built binaries from the [releases page](https://github.com/simon-curtis/jitzu/releases):

| Platform | Download |
|----------|----------|
| Linux x64 | `jitzu-v0.1.0-linux-x64.zip` |
| Windows x64 | `jitzu-v0.1.0-win-x64.zip` |
| macOS x64 | `jitzu-v0.1.0-osx-x64.zip` |
| macOS ARM | `jitzu-v0.1.0-osx-arm64.zip` |

Extract the zip and add `jz` (interpreter) and/or `jzsh` (shell) to your PATH.

## Quick Start

Run a script:
```sh
jz myscript.jz
```

Use Jitzu as an interactive shell:
```sh
jzsh
```

## Why Jitzu?

| Feature | Jitzu |
|---------|-------|
| Runtime | .NET 10 |
| Execution | Interpreted or Shell |
| NuGet Support | Load any .NET package |
| Syntax | Inspired by Rust, Go, Zig, C# |
| Philosophy | Code should be fun! |

## Example Code

```jitzu
// myscript.jz

fun add(x: Int, y: Int) -> Result<Int, String> {
    Ok(x + y)
}

let x = 10
let y = 20
let z = try add(x, y) or 30

`The sum of {x} and {y} is {z}`
```

Run it:
```terminal
$ jz myscript.jz
The sum of 10 and 20 is 30
```

## Jitzu Shell (jzsh)

The interactive shell supports:
- Command chaining (`|`, `&&`, `||`)
- I/O redirection (`>`, `>>`, `<`)
- Job control (background jobs, `fg`, `bg`)
- Glob expansion
- Tab completion
- Command history

```terminal
$ jzsh
jzsh$ echo "Hello, World!"
Hello, World!
jzsh$ ls -la
...
jzsh$ exit
```

## Documentation

Full documentation is available at [jitzu.simoncurtis.dev/docs](https://jitzu.simoncurtis.dev/docs).

## Building from Source

Requirements: .NET 10 SDK

```sh
# Build
dotnet build

# Run interpreter
dotnet run --project Jitzu.Interpreter -- ../Tests/script.jz

# Run shell
dotnet run --project Jitzu.Shell

# Run tests
dotnet test
```

## License

MIT License - see [LICENSE](LICENSE)
