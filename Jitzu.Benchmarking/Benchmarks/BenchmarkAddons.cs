using System.Text.Json.Serialization;

namespace Jitzu.Benchmarking.Benchmarks;

public record BenchmarkAddons
{
    [JsonPropertyName("webServer")]
    public WebServerAddon? WebServer { get; init; }
}