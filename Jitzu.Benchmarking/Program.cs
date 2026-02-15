using ConsoleTableExt;
using Jitzu.Benchmarking;
using Jitzu.Benchmarking.Benchmarks;
using Jitzu.Benchmarking.Display;

var benchmarkArgs = BenchmarkArgs.Parse(args);
var results = new List<RunResult>();

foreach (var directory in Directory.GetDirectories(@"D:\git\jitzu\Jitzu.Benchmarking\Scripts"))
{
    if (benchmarkArgs.Tests is { } tests && !tests.Contains(Path.GetFileName(directory)))
        continue;

    var benchmark = new Benchmark(directory, benchmarkArgs);
    await benchmark.RunAsync(results);
}

var summary = SummariseResults(results);
var table = CreateTableFromResults(summary);

Console.WriteLine(SystemInfoCollector.GetSystemInfo());
Console.WriteLine();
Console.WriteLine(table);

return;

static ResultSummary[] SummariseResults(List<RunResult> results)
{
    return results
        .GroupBy(_ => new { _.Script, _.Iterations, _.RunName })
        .Select(static r =>
        {
            var (mean, err, stdDev) = CalculateAverages(r.Select(_ => _.Time).ToArray());
            return new ResultSummary
            {
                Run = r.Key.RunName,
                Script = r.Key.Script,
                Iterations = r.Key.Iterations,
                MeanTime = mean,
                Error = err,
                StdDev = stdDev,
            };
        })
        .ToArray();
}

static string CreateTableFromResults(ResultSummary[] results)
{
    var rows = new List<Row>();

    foreach (var group in results.GroupBy(_ => _.Run))
    {
        var minMeanTime = group.Min(_ => _.MeanTime);
        var rank = 1;
        foreach (var result in group.OrderBy(_ => _.MeanTime))
        {
            var ratio = result.MeanTime.TotalNanoseconds / minMeanTime.TotalNanoseconds;
            var difference = result.MeanTime.TotalNanoseconds - minMeanTime.TotalNanoseconds;

            rows.Add(
                new Row
                {
                    Script = result.Script,
                    Args = result.Run,
                    N = result.Iterations,
                    Rank = rank++,
                    Mean = FormatTime(result.MeanTime.TotalNanoseconds),
                    Error = FormatTime(result.Error.TotalNanoseconds),
                    StdDev = FormatTime(result.StdDev.TotalNanoseconds),
                    Ratio = ratio.ToString("#0.00"),
                    Difference = FormatTime(difference),
                });
        }
    }

    return ConsoleTableBuilder
        .From(rows)
        .WithFormat(ConsoleTableBuilderFormat.MarkDown)
        .WithTextAlignment(
            new Dictionary<int, TextAligntment>
            {
                { 1, TextAligntment.Right },
                { 2, TextAligntment.Right },
                { 3, TextAligntment.Right },
                { 4, TextAligntment.Right },
                { 5, TextAligntment.Right },
                { 6, TextAligntment.Right },
                { 7, TextAligntment.Right },
                { 8, TextAligntment.Right },
            })
        .Export()
        .ToString();
}

static (TimeSpan Mean, TimeSpan Error, TimeSpan StdDev) CalculateAverages(TimeSpan[] times)
{
    if (times.Length == 0)
        return default;

    // Calculate mean
    double mean = times.Average(t => t.TotalMilliseconds);

    // Calculate standard deviation
    double sumOfSquaredDifferences = times.Sum(x => Math.Pow(x.TotalMilliseconds - mean, 2));
    double stdDev = Math.Sqrt(sumOfSquaredDifferences / times.Length);

    // Calculate standard error of the mean
    double error = stdDev / Math.Sqrt(times.Length);

    return (
        TimeSpan.FromMilliseconds(mean),
        TimeSpan.FromMilliseconds(error),
        TimeSpan.FromMilliseconds(stdDev)
    );
}

static string FormatTime(double nanoseconds)
{
    const double nsPerUs = 1_000;
    const double nsPerMs = 1_000_000;
    const double nsPerS = 1_000_000_000;
    const double nsPerMin = 60_000_000_000;
    const double nsPerHour = 3_600_000_000_000;
    const double nsPerDay = 86_400_000_000_000;

    return nanoseconds switch
    {
        < 1 => $"{nanoseconds:F3} ns",
        < nsPerUs => $"{nanoseconds:F1} ns",
        < nsPerMs => $"{nanoseconds / nsPerUs:F3} us",
        < nsPerS => $"{nanoseconds / nsPerMs:F3} ms",
        < nsPerMin => $"{nanoseconds / nsPerS:F3} s",
        < nsPerHour => $"{nanoseconds / nsPerMin:F1} min",
        < nsPerDay => $"{nanoseconds / nsPerHour:F1} h",
        _ => $"{nanoseconds / nsPerDay:F1} day"
    };
}

namespace Jitzu.Benchmarking
{
    internal record Row
    {
        public required string Script { get; init; }
        public required string? Args { get; init; }
        public required int N { get; init; }
        public required int Rank { get; init; }
        public required string Mean { get; init; }
        public required string Error { get; init; }
        public required string StdDev { get; init; }
        public required string Ratio { get; init; }
        public required string Difference { get; init; }
    }
}