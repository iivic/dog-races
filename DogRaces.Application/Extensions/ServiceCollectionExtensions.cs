using DogRaces.Application.Interfaces;
using DogRaces.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DogRaces.Application.Extensions;

/// <summary>
/// Service collection extensions for registering application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add application layer services to dependency injection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Register wallet service as singleton (in-memory state needs to persist)
        services.AddSingleton<IWalletService, WalletService>();

        return services;
    }
}