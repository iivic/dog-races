using DogRaces.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DogRaces.Infrastructure.Database;

/// <summary>
/// Database configuration and service registration
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Add database services using configuration
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");

        return AddDatabase(services, connectionString);
    }

    /// <summary>
    /// Add database services with explicit connection string
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        services.AddDbContext<DogRacesContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Register the interface
        services.AddScoped<IDogRacesContext>(provider => provider.GetRequiredService<DogRacesContext>());

        return services;
    }
}
