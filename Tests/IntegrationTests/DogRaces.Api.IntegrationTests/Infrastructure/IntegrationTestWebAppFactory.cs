using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DogRaces.Infrastructure.Database;
using Testcontainers.PostgreSql;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("dograces_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .WithLogger(LoggerFactory.Create(logging => logging.ClearProviders()).CreateLogger<PostgreSqlContainer>())
        .Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DogRacesContext>();
        await context.Database.MigrateAsync();
        
        var testDataSeedService = scope.ServiceProvider.GetRequiredService<TestDataSeedService>();
        await testDataSeedService.SeedTestData(CancellationToken.None);
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(ConfigureDatabaseServices);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Error);

            // Suppress Entity Framework logs
            logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
            logging.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
            logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);

            // Suppress Testcontainers logs
            logging.AddFilter("testcontainers.org", LogLevel.None);

            // Suppress DogRaces application logs
            logging.AddFilter("DogRaces.Application", LogLevel.Error);
            logging.AddFilter("DogRaces.BackgroundServices", LogLevel.None);
        });
    }

    private void ConfigureDatabaseServices(IServiceCollection services)
    {
        // Remove existing database context
        var serviceDescriptor = services
            .SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == typeof(DbContextOptions<DogRacesContext>));

        if (serviceDescriptor != null)
        {
            services.Remove(serviceDescriptor);
        }

        // Add test database context
        services.AddDbContext<DogRacesContext>(options =>
        {
            options
                .UseNpgsql(
                    connectionString: _postgreSqlContainer.GetConnectionString(),
                    npgsqlOptionsAction: npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(DogRacesContext).Assembly.FullName);
                        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    })
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });

        // Add test data seeding service
        services.AddScoped<TestDataSeedService>();
    }
}