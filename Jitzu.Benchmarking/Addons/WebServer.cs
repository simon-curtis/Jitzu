using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jitzu.Benchmarking.Addons;

public class Startup(IConfiguration configuration) : IStartup
{
    public IConfiguration Configuration { get; } = configuration;

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        return services.BuildServiceProvider();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet(
                "/Hello/{name}", (string name) => Results.Json(
                    new TestRecord
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Something = Random.Shared.Next()
                    }));
        });
    }
}

public record TestRecord
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required int Something { get; init; }
}