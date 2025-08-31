using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Races.Queries.GetActiveRaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
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
    public async Task GetActiveRaces_ShouldIncludeRaceOddsForEachRace()
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
            Assert.NotNull(race.RaceOdds);
            Assert.NotEmpty(race.RaceOdds);
            
            // Should have odds for all combinations (6 selections × 3 bet types = 18 total)
            Assert.Equal(18, race.RaceOdds.Count);
            
            // Verify each race odds has required properties
            Assert.All(race.RaceOdds, raceOdds =>
            {
                Assert.True(raceOdds.Id > 0); // Should have valid ID
                Assert.InRange(raceOdds.Selection, 1, 6); // Selection should be 1-6
                Assert.True(raceOdds.Odds > 1.0m); // All odds should be > 1.0
                Assert.Contains(raceOdds.BetType, new[] { "Winner", "Top2", "Top3" }); // Valid bet types
            });
            
            // Verify we have all selections for each bet type
            var winnerOdds = race.RaceOdds.Where(ro => ro.BetType == "Winner").ToList();
            var top2Odds = race.RaceOdds.Where(ro => ro.BetType == "Top2").ToList();
            var top3Odds = race.RaceOdds.Where(ro => ro.BetType == "Top3").ToList();
            
            Assert.Equal(6, winnerOdds.Count); // 6 selections for Winner bets
            Assert.Equal(6, top2Odds.Count);   // 6 selections for Top2 bets  
            Assert.Equal(6, top3Odds.Count);   // 6 selections for Top3 bets
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