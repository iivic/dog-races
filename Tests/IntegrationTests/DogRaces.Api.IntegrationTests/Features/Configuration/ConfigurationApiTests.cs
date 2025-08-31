using System.Net;
using System.Net.Http.Json;
using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Configuration;

public class ConfigurationApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    public ConfigurationApiTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetGlobalConfiguration_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/configuration");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGlobalConfiguration_ShouldReturnValidConfiguration()
    {
        // Act
        var response = await _httpClient.GetFromJsonAsync<GetGlobalConfigurationResponse>("/api/configuration");

        // Assert
        Assert.NotNull(response);
        Assert.True(response.MinTicketStake > 0);
        Assert.True(response.MaxTicketWin > 0);
        Assert.True(response.MinNumberOfActiveRounds > 0);
        Assert.True(response.MaxTicketWin > response.MinTicketStake);
    }

    [Fact]
    public async Task GetGlobalConfiguration_ShouldReturnSeededValues()
    {
        // Act
        var response = await _httpClient.GetFromJsonAsync<GetGlobalConfigurationResponse>("/api/configuration");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(1.0m, response.MinTicketStake);
        Assert.Equal(10000.0m, response.MaxTicketWin);
        Assert.Equal(7, response.MinNumberOfActiveRounds);
    }
}
