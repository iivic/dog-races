using System.Net;
using System.Net.Http.Json;
using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Races.Queries.GetActiveRaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Races;

public class RacesApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    public RacesApiTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetActiveRaces_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/races/active");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveRaces_ShouldReturnValidJsonStructure()
    {
        // Act
        var response = await _httpClient.GetFromJsonAsync<GetActiveRacesResponse>("/api/races/active");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Races);
        Assert.True(response.TotalCount >= 0);
    }

    [Fact]
    public async Task GetActiveRaces_WithTestRace_ShouldIncludeRaceInResponse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var testDataService = scope.ServiceProvider.GetRequiredService<TestDataSeedService>();
        var testRace = await testDataService.CreateTestRace(DateTimeOffset.UtcNow.AddMinutes(5));

        // Act
        var response = await _httpClient.GetFromJsonAsync<GetActiveRacesResponse>("/api/races/active");

        // Assert
        Assert.NotNull(response);
        Assert.Contains(response.Races, r => r.Id == testRace.Id);
        Assert.Contains(response.Races, r => r.RaceName == testRace.RaceName);
    }

    [Fact]
    public async Task GetActiveRaces_ShouldIncludeOddsForEachRace()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var testDataService = scope.ServiceProvider.GetRequiredService<TestDataSeedService>();
        await testDataService.CreateTestRace(DateTimeOffset.UtcNow.AddMinutes(5));

        // Act
        var response = await _httpClient.GetFromJsonAsync<GetActiveRacesResponse>("/api/races/active");

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Races);
        
        foreach (var race in response.Races)
        {
            Assert.NotNull(race.Odds);
            Assert.Equal(6, race.Odds.Count); // Should have odds for all 6 dogs
            Assert.All(race.Odds.Values, odds => Assert.True(odds > 1.0m)); // All odds should be > 1.0
        }
    }

    [Fact]
    public async Task GetRacesCount_ShouldReturnSuccessStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/races/count");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRacesCount_ShouldReturnValidNumber()
    {
        // Act
        var response = await _httpClient.GetFromJsonAsync<int>("/api/races/count");

        // Assert
        Assert.True(response >= 0);
    }
}
