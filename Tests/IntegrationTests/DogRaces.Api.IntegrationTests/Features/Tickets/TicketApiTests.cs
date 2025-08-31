using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Tickets.Commands.PlaceBet;
using DogRaces.Application.Features.Tickets.Queries.GetTickets;
using DogRaces.Application.Features.Wallet.Commands.ResetWallet;
using DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Features.Tickets;

public class TicketApiTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationTestWebAppFactory _factory;

    public TicketApiTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task PlaceBet_WithValidSingleBet_ShouldReturnSuccess()
    {
        // Arrange - Reset wallet to ensure clean state
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        // Ensure we have races and odds available
        await EnsureRacesAndOddsExist();

        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 5.0m,
            Bets: [new BetRequest(RaceOddsId: 1)]
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.TicketId);
        Assert.Contains("Bet placed successfully", result.Message);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public async Task PlaceBet_WithMultipleBets_ShouldReturnSuccess()
    {
        // Arrange - Reset wallet to ensure clean state
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        // Ensure we have races and odds available
        await EnsureRacesAndOddsExist();

        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 10.0m,
            Bets: [
                new BetRequest(RaceOddsId: 1),
                new BetRequest(RaceOddsId: 8) // Different race, different bet type
            ]
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.TicketId);
        Assert.Contains("Bet placed successfully", result.Message);
        Assert.Contains("Potential win:", result.Message);
        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public async Task PlaceBet_WithInvalidStakeTooLow_ShouldReturnValidationError()
    {
        // Arrange
        await EnsureRacesAndOddsExist();

        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 0.5m, // Below minimum stake of 1.0
            Bets: [new BetRequest(RaceOddsId: 1)]
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.TicketId);
        Assert.Equal("Ticket validation failed", result.Message);
        Assert.Contains(result.ValidationErrors, error => error.Contains("Minimum stake is"));
    }

    [Fact]
    public async Task PlaceBet_WithNonExistentRaceOdds_ShouldReturnValidationError()
    {
        // Arrange
        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 5.0m,
            Bets: [new BetRequest(RaceOddsId: 999)] // Non-existent odds ID
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.TicketId);
        Assert.Equal("Ticket validation failed", result.Message);
        Assert.Contains(result.ValidationErrors, error => error.Contains("Race odds with ID 999 not found"));
    }

    [Fact]
    public async Task PlaceBet_WithInsufficientFunds_ShouldReturnInsufficientFundsError()
    {
        // Arrange - Reset wallet with low balance
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(2m));

        await EnsureRacesAndOddsExist();

        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 10.0m, // More than available balance
            Bets: [new BetRequest(RaceOddsId: 1)]
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert - Insufficient funds is handled during wallet operation, not validation
        // So it returns BadRequest when validation passes but wallet operation fails
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.TicketId);
        Assert.Equal("Insufficient funds", result.Message);
        Assert.Contains(result.ValidationErrors, error => error.Contains("Unable to reserve"));
    }

    [Fact]
    public async Task PlaceBet_WithEmptyBets_ShouldReturnValidationError()
    {
        // Arrange
        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 5.0m,
            Bets: [] // Empty bets list
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.TicketId);
        Assert.Equal("Ticket validation failed", result.Message);
        Assert.Contains(result.ValidationErrors, error => error.Contains("At least one bet is required"));
    }

    [Fact]
    public async Task PlaceBet_ShouldUpdateWalletBalance()
    {
        // Arrange - Reset wallet to known state
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        // Get initial balance
        var initialBalanceResponse = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(initialBalanceResponse);
        var initialBalance = initialBalanceResponse.AvailableBalance;

        await EnsureRacesAndOddsExist();

        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 10.0m,
            Bets: [new BetRequest(RaceOddsId: 1)]
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert bet was successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Check wallet balance was reduced
        var finalBalanceResponse = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(finalBalanceResponse);

        Assert.Equal(initialBalance - 10.0m, finalBalanceResponse.AvailableBalance);
        Assert.Equal(0m, finalBalanceResponse.ReservedAmount); // Should be committed, not reserved
    }

    [Fact]
    public async Task PlaceBet_WithValidationFailure_ShouldNotUpdateWalletBalance()
    {
        // Arrange - Reset wallet to known state
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));

        // Get initial balance
        var initialBalanceResponse = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(initialBalanceResponse);
        var initialBalance = initialBalanceResponse.AvailableBalance;

        var placeBetRequest = new PlaceBetCommand(
            TotalStake: 0.5m, // Invalid stake (too low)
            Bets: [new BetRequest(RaceOddsId: 1)]
        );

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", placeBetRequest);

        // Assert bet failed
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Check wallet balance unchanged
        var finalBalanceResponse = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(finalBalanceResponse);

        Assert.Equal(initialBalance, finalBalanceResponse.AvailableBalance);
        Assert.Equal(0m, finalBalanceResponse.ReservedAmount);
    }

    [Fact]
    public async Task PlaceBet_WorkflowIntegration_ShouldWorkEndToEnd()
    {
        // Arrange - Complete workflow test
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(50m));
        await EnsureRacesAndOddsExist();

        // Act & Assert - Place multiple bets in sequence

        // First bet
        var firstBet = new PlaceBetCommand(10m, [new BetRequest(1)]);
        var firstResponse = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", firstBet);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var firstResult = await firstResponse.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(firstResult);
        Assert.True(firstResult.Success);

        // Check balance after first bet
        var balanceAfterFirst = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(balanceAfterFirst);
        Assert.Equal(40m, balanceAfterFirst.AvailableBalance);

        // Second bet
        var secondBet = new PlaceBetCommand(15m, [new BetRequest(2)]);
        var secondResponse = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", secondBet);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        // Check final balance
        var finalBalance = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(finalBalance);
        Assert.Equal(25m, finalBalance.AvailableBalance);

        // Try bet with insufficient funds
        var insufficientBet = new PlaceBetCommand(30m, [new BetRequest(3)]);
        var insufficientResponse = await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", insufficientBet);

        var insufficientResult = await insufficientResponse.Content.ReadFromJsonAsync<PlaceBetResponse>();
        Assert.NotNull(insufficientResult);
        Assert.False(insufficientResult.Success);
        Assert.Equal("Insufficient funds", insufficientResult.Message);

        // Balance should remain unchanged after failed bet
        var balanceAfterFailed = await _httpClient.GetFromJsonAsync<GetWalletStatusResponse>("/api/wallet/balance");
        Assert.NotNull(balanceAfterFailed);
        Assert.Equal(25m, balanceAfterFailed.AvailableBalance);
    }

    [Fact]
    public async Task GetTickets_WithMultipleTickets_ShouldReturnAllTickets()
    {
        // Arrange - Reset wallet and create test data
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));
        await EnsureRacesAndOddsExist();

        // Place multiple tickets
        var firstBet = new PlaceBetCommand(10m, [new BetRequest(1)]);
        var secondBet = new PlaceBetCommand(15m, [new BetRequest(2)]);

        await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", firstBet);
        await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", secondBet);

        // Act
        var response = await _httpClient.GetAsync("/api/tickets");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetTicketsResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Tickets.Count());

        // Verify ticket properties
        var tickets = result.Tickets.ToList();
        Assert.All(tickets, ticket =>
        {
            Assert.NotEqual(Guid.Empty, ticket.Id);
            Assert.Equal("Success", ticket.Status);
            Assert.True(ticket.TotalStake > 0);
            Assert.True(ticket.CreatedAt <= DateTimeOffset.UtcNow);
            Assert.NotEmpty(ticket.Bets);
        });

        // Verify tickets are ordered by creation date (newest first)
        var orderedTickets = tickets.OrderByDescending(t => t.CreatedAt).ToList();
        Assert.Equal(orderedTickets.Select(t => t.Id), tickets.Select(t => t.Id));
    }

    [Fact]
    public async Task GetTickets_WithStatusFilter_ShouldReturnFilteredResults()
    {
        // Arrange - This test assumes we can create tickets with different statuses
        // For now, we can only create Success tickets through the API
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));
        await EnsureRacesAndOddsExist();

        // Place a ticket
        var bet = new PlaceBetCommand(10m, [new BetRequest(1)]);
        await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", bet);

        // Act - Filter by Success status
        var response = await _httpClient.GetAsync("/api/tickets?status=Success");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetTicketsResponse>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.All(result.Tickets, ticket => Assert.Equal("Success", ticket.Status));
    }

    [Fact]
    public async Task GetTickets_WithInvalidStatusFilter_ShouldReturnAllTickets()
    {
        // Arrange
        await _httpClient.PostAsJsonAsync("/api/wallet/reset", new ResetWalletCommand(100m));
        await EnsureRacesAndOddsExist();

        var bet = new PlaceBetCommand(10m, [new BetRequest(1)]);
        await _httpClient.PostAsJsonAsync("/api/tickets/place-bet", bet);

        // Act - Use invalid status filter
        var response = await _httpClient.GetAsync("/api/tickets?status=InvalidStatus");

        // Assert - Should return all tickets (filter ignored)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetTicketsResponse>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
    }

    /// <summary>
    /// Helper method to ensure races and odds exist for testing
    /// </summary>
    private async Task EnsureRacesAndOddsExist()
    {
        using var scope = _factory.Services.CreateScope();
        var testDataService = scope.ServiceProvider.GetRequiredService<TestDataSeedService>();

        // Create test races with odds
        await testDataService.CreateTestRace(DateTimeOffset.UtcNow.AddMinutes(10));
        await testDataService.CreateTestRace(DateTimeOffset.UtcNow.AddMinutes(15));
    }
}