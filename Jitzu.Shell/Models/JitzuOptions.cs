using Clap.Net;

namespace Jitzu.Shell.Models;

[Command(
    About = "Jitzu - A fast and flexible script execution engine",
    LongAbout = "Jitzu is a modern scripting language literally designed to be full to the brim with syntax sugar, making it fun(?) to write scripts and stuff."
)]
public partial class JitzuOptions
{
    // Shell options
    [Arg(Long = "splash", Negation = true)]
    public bool Splash { get; init; } = true;

    [Arg(Short = 'c', Long = "command")]
    public string? Command { get; init; }

    [Arg(Long = "sudo-exec")]
    public string? SudoExec { get; init; }

    [Arg(Long = "sudo-shell")]
    public bool SudoShell { get; init; }

    [Arg(Long = "sudo-login")]
    public bool SudoLogin { get; init; }

    [Arg(Long = "parent-pid")]
    public int ParentPid { get; init; }

    [Arg(Long = "sudo-preserve-env")]
    public bool SudoPreserveEnv { get; init; }

    [Arg(Long = "persist", Negation = true, Help = "Disable reading/writing history and alias files")]
    public bool Persist { get; init; } = true;

    // Interpreter options
    [Arg(Short = 'd', Long = "debug")]
    public bool Debug { get; init; }

    [Arg(Short = 't', Long = "telemetry")]
    public bool Telemetry { get; init; }

    [Arg(Short = 'b', Help = "If provided, the bytecode of the application will be written to it")]
    public string? BytecodeOutputPath { get; set; }

    [Arg(Long = "install-path")]
    public bool InstallPath { get; init; }

    // Positional args
    public string? ScriptPath { get; init; }

    [Arg(Help = "Additional arguments to pass to the script")]
    public string[] ScriptArgs { get; init; } = [];
}
