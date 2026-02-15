using Clap.Net;

namespace Jitzu.Benchmarking;

[Command]
public partial class BenchmarkArgs
{
    [Arg(Short = 't', Long = "tests")]
    public string[]? Tests { get; init; }

    [Arg(Short = 'e', Long = "extensions")]
    public string[] Extensions { get; private init; } = ["jz", "ps1", "py"];
}