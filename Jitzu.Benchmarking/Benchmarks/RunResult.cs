namespace Jitzu.Benchmarking.Benchmarks;

public record RunResult
{
    public required string Script { get; set; }
    public required string RunName { get; init; }
    public required int Iterations { get; init; }
    public TimeSpan Time { get; init; }
}