using DogRaces.Application.Extensions;
using DogRaces.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DogRaces.Infrastructure.Extensions;

/// <summary>
/// Infrastructure service collection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all infrastructure services (database + application services)
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add database services
        services.AddDatabase(configuration);

        // Add application services (including wallet and MediatR)
        services.AddApplicationServices();

        return services;
    }
}