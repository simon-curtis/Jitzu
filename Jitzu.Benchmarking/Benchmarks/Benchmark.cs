using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
using Jitzu.Benchmarking.Addons;
using Microsoft.AspNetCore.Hosting;

namespace Jitzu.Benchmarking.Benchmarks;

public class Benchmark
{
    private readonly string _directoryName;
    private readonly BenchmarkArgs _args;
    private readonly BenchmarkConfig _config;

    public Benchmark(string directoryName, BenchmarkArgs args)
    {
        _directoryName = directoryName;
        _args = args;

        var configPath = Path.Combine(directoryName, "config.json");
        _config = File.Exists(configPath)
            ? JsonSerializer.Deserialize<BenchmarkConfig>(File.ReadAllText(configPath))!
            : new BenchmarkConfig();
    }

    public async Task RunAsync(List<RunResult> results)
    {
        var scripts = Directory.GetFiles(_directoryName);
        var disposables = new List<IDisposable>();

        if (_config.AddOns?.WebServer is { } webServerAddon)
        {
            var webBuilder = new WebHostBuilder();
            webBuilder.UseKestrel(options => { options.ListenLocalhost(webServerAddon.Port); });
            webBuilder.UseStartup<Startup>();
            var webHost = webBuilder.Build();

            Console.WriteLine("Starting webserver");
            await webHost.StartAsync();
            disposables.Add(webHost);
        }

        if (_config.Runs is { Length: > 0 } runs)
        {
            foreach (var run in runs)
            foreach (var script in scripts)
                await RunScriptAsync(script, _config.Iterations, results, [run.ToString(), .._config.Args]);
        }
        else
        {
            foreach (var script in scripts)
                await RunScriptAsync(script, _config.Iterations, results, _config.Args);
        }

        foreach (var disposable in disposables)
            disposable.Dispose();
    }

    private async Task RunScriptAsync(
        string script,
        int iterations,
        List<RunResult> results,
        params string[] args)
    {
        var scriptName = Path.GetFileName(script);
        var extension = scriptName.Split('.').Last();
        if (!_args.Extensions.Contains(extension)) return;

        var command = extension switch
        {
            "jz" => new Command(@"D:\git\jitzu\Jitzu.Shell\bin\Publish\jz").WithArguments([script, ..args]),
            "py" => new Command("python3").WithArguments([script, ..args]),
            "ps1" => new Command("pwsh").WithArguments(["-noprofile", script, ..args]),
            _ => throw new Exception($"Unknown extension: {extension}")
        };

        var runName = string.Join(" ", args);
        var program = Path.GetFileName(command.TargetFilePath);
        Console.WriteLine($"Starting {program} \"{script}\" {runName}: ");

        var totalRunTime = 0;
        for (int i = 0; i < iterations; i++)
        {
            Console.Write($"  > Iteration {i:#000}");

            var result = await command
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            Console.WriteLine($" Returned: {result.ExitCode}, Took: {result.RunTime}");

            totalRunTime += result.RunTime.Milliseconds;
            results.Add(
                new RunResult
                {
                    Script = scriptName,
                    Iterations = iterations,
                    RunName = runName,
                    Time = result.RunTime,
                });
        }

        Console.WriteLine($"Mean time {totalRunTime / iterations:F}");
        Console.WriteLine();
    }
}