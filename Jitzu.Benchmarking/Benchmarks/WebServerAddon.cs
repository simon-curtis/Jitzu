using System.Text.Json.Serialization;

namespace Jitzu.Benchmarking.Benchmarks;

public record WebServerAddon
{
    [JsonPropertyName("port")]
    public required int Port { get; init; }
}