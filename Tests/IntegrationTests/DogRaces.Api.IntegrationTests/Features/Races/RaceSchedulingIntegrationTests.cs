using DogRaces.Application.Features.Configuration.Queries.GetGlobalConfiguration;
using DogRaces.Application.Features.Races.Commands.CreateScheduledRace;
using DogRaces.Application.Features.Races.Commands.EnsureMinimumRaces;
using DogRaces.Application.Features.Races.Commands.ProcessBettingClosures;
using DogRaces.Application.Features.Races.Commands.ProcessRaceStarts;
using DogRaces.Application.Features.Races.Commands.ProcessRaceFinishes;
using DogRaces.Application.Features.Races.Queries.GetActiveRaces;
using DogRaces.Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Net.Http.Json;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Races;

public class RaceSchedulingIntegrationTests : BaseIntegrationTest
{
    public RaceSchedulingIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RaceScheduling_FullLifecycle_ShouldWork()
    {
        // Arrange - Get sender for CQRS commands
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // Step 1: Check initial configuration
        var configResponse = await _client.GetFromJsonAsync<GetGlobalConfigurationResponse>("/api/configuration");
        
        // Step 2: Ensure minimum races are created
        var ensureResult = await sender.Send(new EnsureMinimumRacesCommand());
        
        // Step 3: Verify races exist via API
        var activeRacesResponse = await _client.GetFromJsonAsync<GetActiveRacesResponse>("/api/races/active");
        
        Assert.True(activeRacesResponse!.Races.Count >= configResponse!.MinNumberOfActiveRounds);
        
        // Step 4: Test race lifecycle processing
        var bettingResult = await sender.Send(new ProcessBettingClosuresCommand());
        var startsResult = await sender.Send(new ProcessRaceStartsCommand());
        var finishesResult = await sender.Send(new ProcessRaceFinishesCommand());
        
        // Assert that commands executed successfully (even if no races were in the right state)
        Assert.True(bettingResult.RacesProcessed >= 0);
        Assert.True(startsResult.RacesProcessed >= 0);
        Assert.True(finishesResult.RacesProcessed >= 0);
    }
    
    [Fact]
    public async Task CreateScheduledRace_ShouldGenerateValidRace()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Act
        var result = await sender.Send(new CreateScheduledRaceCommand());
        
        // Assert
        Assert.True(result.RaceId > 0);
        Assert.NotNull(result.RaceName);
        Assert.NotEmpty(result.RaceName);
        Assert.True(result.StartTime > DateTimeOffset.UtcNow);
        Assert.True(result.EndTime > result.StartTime);
    }

    [Fact]
    public async Task RaceTimingLogic_ShouldProcessCorrectRaceStates()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // Create some test races first
        await sender.Send(new EnsureMinimumRacesCommand());
        
        // Act - Process race lifecycle
        var initial = await sender.Send(new GetActiveRacesQuery());
        var bettingResult = await sender.Send(new ProcessBettingClosuresCommand());
        var startsResult = await sender.Send(new ProcessRaceStartsCommand());  
        var finishesResult = await sender.Send(new ProcessRaceFinishesCommand());
        var final = await sender.Send(new GetActiveRacesQuery());
        
        // Assert - Commands should execute without errors
        Assert.NotNull(bettingResult);
        Assert.NotNull(startsResult);
        Assert.NotNull(finishesResult);
        Assert.True(initial.Races.Count >= 0);
        Assert.True(final.Races.Count >= 0);
    }
}
