using DogRaces.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly IntegrationTestWebAppFactory _factory;
    protected HttpClient _client;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _factory = factory;
        // Create a new client for each test to ensure isolation
        _client = null!; // Will be initialized in InitializeAsync
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        _client.Dispose();
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        // Create a new client for each test and reset wallet to ensure clean state
        _client = _factory.CreateClient();

        // Reset the wallet to default state before each test
        var walletService = _scope.ServiceProvider.GetRequiredService<IWalletService>();
        walletService.ResetWallet(100m);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Create JSON content for POST requests
    /// </summary>
    protected static StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Deserialize JSON response
    /// </summary>
    protected static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })!;
    }
}