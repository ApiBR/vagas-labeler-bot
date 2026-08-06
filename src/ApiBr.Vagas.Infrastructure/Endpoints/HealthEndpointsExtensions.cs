using ApiBr.Vagas.Application.Ports;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBr.Vagas.Infrastructure.Endpoints;

public static class HealthEndpointsExtensions
{
    public static IEndpointRouteBuilder MapVagasHealthEndpoints(this IEndpointRouteBuilder app, string prefix = "")
    {
        app.MapGet($"{prefix}/health", () => Results.Ok(new { status = "healthy" }));

        // Dependency checks (MariaDB, LavinMQ, GitHub API) are wired in once Infrastructure gains those clients.
        app.MapGet($"{prefix}/health/ready", () => Results.Ok(new { status = "ready" }));

        app.MapGet($"{prefix}/version", (IVersionProvider versionProvider) =>
            Results.Ok(new { version = versionProvider.InformationalVersion }));

        return app;
    }

    public static IApplicationBuilder UseApiVersionHeader(this WebApplication app)
    {
        var version = app.Services.GetRequiredService<IVersionProvider>().InformationalVersion;

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Api-Version"] = version;
            await next(context);
        });

        return app;
    }
}
