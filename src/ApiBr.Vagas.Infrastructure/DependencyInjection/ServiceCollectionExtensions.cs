using ApiBr.Vagas.Application.Ports;
using ApiBr.Vagas.Infrastructure.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBr.Vagas.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IVersionProvider, AssemblyVersionProvider>();

        return services;
    }
}
