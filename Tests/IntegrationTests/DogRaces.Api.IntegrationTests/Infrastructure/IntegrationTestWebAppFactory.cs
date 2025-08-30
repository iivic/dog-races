using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using DogRaces.Application.Interfaces;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        // No special initialization needed for in-memory wallet
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        // No special cleanup needed for in-memory wallet
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override wallet service to use test-specific singleton for state persistence across requests
            services.Remove(services.Single(d => d.ServiceType == typeof(IWalletService)));
            services.AddSingleton<IWalletService, TestWalletService>();
        });

        builder.UseEnvironment("Testing");
    }
}
