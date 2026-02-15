using Clap.Net;

namespace Jitzu.Interpreter.Infrastructure.Configuration;

[Command(
    About = "Jitzu - A fast and flexible script execution engine",
    LongAbout = "Jitzu is a modern scripting language literally designed to be full to the brim with syntax sugar, making it fun(?) to write scripts and stuff."
)]
public sealed partial class JitzuCli
{
    [Arg(Long = "install-path")]
    public bool InstallPath { get; init; }

    [Arg(Short = 'd', Long = "debug")]
    public bool Debug { get; init; }

    [Arg(Short = 't', Long = "telemetry")]
    public bool Telemetry { get; init; }

    [Arg(Help = "Path to the script file or entry point to execute")]
    public string? EntryPoint { get; init; }

    [Arg(Help = "Additional arguments to pass to the script")]
    public string[] ScriptArgs { get; init; } = [];

    [Arg(Short = 'b', Help = "If provided, the bytecode of the application will be written to it")]
    public string? BytecodeOutputPath { get; set; }

    [Command(Subcommand = true)]
    public CliActions? Action { get; init; }
}

[SubCommand]
public partial class CliActions
{
    [Command(Name = "run")]
    public partial class RunAction : CliActions
    {
        [Arg(Help = "Path to the script file or entry point to execute")]
        public required string EntryPoint { get; init; }

        [Arg(Help = "Additional arguments to pass to the script")]
        public string[] ScriptArgs { get; init; } = [];
    }
}