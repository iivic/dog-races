using System.Net.Http.Json;
using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Tickets.Commands.PlaceBet;
using DogRaces.Application.Features.Tickets.Commands.ProcessUnprocessedTickets;
using DogRaces.Application.Features.Wallet.Commands.ResetWallet;
using DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;
using DogRaces.Domain.Entities;
using DogRaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Tickets;

public class TicketProcessingIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationTestWebAppFactory _factory;

    public TicketProcessingIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithWinningTicket_ShouldPayoutToWallet()
    {
        // Arrange - Reset wallet and create test data
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Create a race and place a bet
        var (raceId, ticketId) = await CreateRaceAndPlaceBet(1, BetType.Winner, 10m);
        
        // Finish the race with winning results (selection 1 wins)
        await FinishRaceWithResults(raceId, [1, 2, 3]);
        
        // Process individual bets to set IsWinning flags
        await ProcessBetsForRace(raceId);

        // Act - Process unprocessed tickets
        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command);

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(1, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.True(result.TotalPayouts > 0);

        // Check wallet balance increased
        var walletResponse = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(walletResponse);
        Assert.True(walletResponse.AvailableBalance > 90m); // Should be more than initial 100 - 10 stake

        // Check ticket status
        await VerifyTicketStatus(ticketId, TicketStatus.Won);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithLosingTicket_ShouldNotPayoutToWallet()
    {
        // Arrange - Reset wallet and create test data
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Create a race and place a bet on a losing selection
        var (raceId, ticketId) = await CreateRaceAndPlaceBet(4, BetType.Winner, 10m);
        
        // Finish the race with results where selection 4 loses
        await FinishRaceWithResults(raceId, [1, 2, 3]);
        
        // Process individual bets to set IsWinning flags
        await ProcessBetsForRace(raceId);

        // Act - Process unprocessed tickets
        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command);

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(1, result.LosingTickets);
        Assert.Equal(0m, result.TotalPayouts);

        // Check wallet balance remains at 90 (100 - 10 stake)
        var walletResponse = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(walletResponse);
        Assert.Equal(90m, walletResponse.AvailableBalance);

        // Check ticket status
        await VerifyTicketStatus(ticketId, TicketStatus.Lost);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithMultipleBetsOnTicket_ShouldEvaluateAllBets()
    {
        // Arrange - Reset wallet and create test data
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Create a race and place multiple bets - one winning, one losing (ticket should lose overall)
        var (raceId, ticketId) = await CreateRaceAndPlaceMultipleBets(raceId: 0, [
            (1, BetType.Winner), // This wins (1 is winner)
            (4, BetType.Winner)  // This loses (4 is not winner) - whole ticket loses
        ], 10m);
        
        // Finish the race with results
        await FinishRaceWithResults(raceId, [1, 2, 3]);
        
        // Process individual bets to set IsWinning flags
        await ProcessBetsForRace(raceId);

        // Act - Process unprocessed tickets
        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command);

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets); // Should be 0 because not all bets won
        Assert.Equal(1, result.LosingTickets);
        Assert.Equal(0m, result.TotalPayouts);

        // Check ticket status
        await VerifyTicketStatus(ticketId, TicketStatus.Lost);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithTop2Bet_ShouldEvaluateCorrectly()
    {
        // Arrange - Reset wallet and create test data
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Create a race and place a Top2 bet on second place
        var (raceId, ticketId) = await CreateRaceAndPlaceBet(2, BetType.Top2, 10m);
        
        // Finish the race with results where selection 2 is second
        await FinishRaceWithResults(raceId, [1, 2, 3]);
        
        // Process individual bets to set IsWinning flags
        await ProcessBetsForRace(raceId);

        // Act - Process unprocessed tickets
        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command);

        // Assert
        Assert.Equal(1, result.ProcessedTickets);
        Assert.Equal(1, result.WinningTickets); // Should win because 2 is in top 2
        Assert.Equal(0, result.LosingTickets);
        Assert.True(result.TotalPayouts > 0);

        // Check ticket status
        await VerifyTicketStatus(ticketId, TicketStatus.Won);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithNoUnprocessedTickets_ShouldReturnZero()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // Act - Process when no unprocessed tickets exist
        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command);

        // Assert
        Assert.Equal(0, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.Equal(0m, result.TotalPayouts);
    }

    [Fact]
    public async Task ProcessUnprocessedTickets_WithUnfinishedRaces_ShouldNotProcessTickets()
    {
        // Arrange - Reset wallet and create test data
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        
        // Create a race and place a bet but don't finish the race
        var (raceId, ticketId) = await CreateRaceAndPlaceBet(1, BetType.Winner, 10m);
        // Note: Not finishing the race, so bets won't have IsWinning set

        // Act - Process unprocessed tickets
        var command = new ProcessUnprocessedTicketsCommand();
        var result = await sender.Send(command);

        // Assert - Should not process tickets with unfinished races
        Assert.Equal(0, result.ProcessedTickets);
        Assert.Equal(0, result.WinningTickets);
        Assert.Equal(0, result.LosingTickets);
        Assert.Equal(0m, result.TotalPayouts);

        // Check ticket is still in Success status
        await VerifyTicketStatus(ticketId, TicketStatus.Success);
    }

    /// <summary>
    /// Helper method to create a race and place a single bet
    /// </summary>
    private async Task<(long raceId, Guid ticketId)> CreateRaceAndPlaceBet(int selection, BetType betType, decimal stake)
    {
        // Create a race
        using var scope = _factory.Services.CreateScope();
        var testDataService = scope.ServiceProvider.GetRequiredService<TestDataSeedService>();
        var race = await testDataService.CreateTestRace(DateTimeOffset.UtcNow.AddMinutes(-5));

        // Find the race odds for this selection and bet type
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        var raceOdds = await context.RaceOdds
            .FirstAsync(ro => ro.RaceId == race.Id && ro.Selection == selection && ro.BetType == betType);

        // Place the bet
        var placeBetRequest = new PlaceBetCommand(
            TotalStake: stake,
            Bets: [new BetRequest(RaceOddsId: raceOdds.Id)]
        );

        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);
        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.TicketId);

        return (race.Id, result.TicketId.Value);
    }

    /// <summary>
    /// Helper method to create a race and place multiple bets on a single ticket
    /// </summary>
    private async Task<(long raceId, Guid ticketId)> CreateRaceAndPlaceMultipleBets(long raceId, (int selection, BetType betType)[] bets, decimal stake)
    {
        // Create a race if not provided
        using var scope = _factory.Services.CreateScope();
        Race race;
        
        if (raceId == 0)
        {
            var testDataService = scope.ServiceProvider.GetRequiredService<TestDataSeedService>();
            race = await testDataService.CreateTestRace(DateTimeOffset.UtcNow.AddMinutes(-5));
            raceId = race.Id;
        }

        // Find the race odds for each bet
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        var betRequests = new List<BetRequest>();
        
        foreach (var (selection, betType) in bets)
        {
            var raceOdds = await context.RaceOdds
                .FirstAsync(ro => ro.RaceId == raceId && ro.Selection == selection && ro.BetType == betType);
            
            betRequests.Add(new BetRequest(RaceOddsId: raceOdds.Id));
        }

        // Place the bets
        var placeBetRequest = new PlaceBetCommand(
            TotalStake: stake,
            Bets: betRequests
        );

        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);
        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.TicketId);

        return (raceId, result.TicketId.Value);
    }

    /// <summary>
    /// Helper method to finish a race with specific results
    /// </summary>
    private async Task FinishRaceWithResults(long raceId, int[] results)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        
        var raceEntity = await context.Races.FindAsync(raceId);
        if (raceEntity != null)
        {
            // Close betting, start race, then finish it with results
            raceEntity.CloseBetting();
            raceEntity.StartRace();
            raceEntity.FinishRace();
            
            // Override the results with our specific results
            var resultField = typeof(Race).GetField("_result", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            resultField?.SetValue(raceEntity, results);
            
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Helper method to process bets for a race (sets IsWinning flags)
    /// </summary>
    private async Task ProcessBetsForRace(long raceId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        
        var race = await context.Races
            .Include(r => r.Bets)
            .FirstOrDefaultAsync(r => r.Id == raceId);
            
        if (race?.Result != null)
        {
            foreach (var bet in race.Bets)
            {
                bet.ProcessResult(race.Result);
            }
            
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Helper method to verify ticket status
    /// </summary>
    private async Task VerifyTicketStatus(Guid ticketId, TicketStatus expectedStatus)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DogRaces.Application.Data.IDogRacesContext>();
        
        var ticket = await context.Tickets.FindAsync(ticketId);
        Assert.NotNull(ticket);
        Assert.Equal(expectedStatus, ticket.Status);
    }
}
