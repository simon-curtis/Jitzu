using Clap.Net;

namespace Jitzu.Shell.Models;

[Command]
public partial class ShellOptions
{
    [Arg(Long = "splash", Negation = true)]
    public bool Splash { get; init; } = true;

    public string? ScriptPath { get; init; }

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
}