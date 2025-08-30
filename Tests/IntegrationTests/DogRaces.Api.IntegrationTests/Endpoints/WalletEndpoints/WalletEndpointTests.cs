using DogRaces.Api.IntegrationTests.Infrastructure;
using DogRaces.Application.Features.Wallet.Queries.GetWalletStatus;
using DogRaces.Application.Features.Wallet.Queries.GetTransactionHistory;
using DogRaces.Application.Features.Wallet.Commands.ResetWallet;
using DogRaces.Application.Features.Wallet.Commands.ReserveFunds;
using DogRaces.Application.Features.Wallet.Commands.ReleaseFunds;
using System.Net.Http.Json;
using Xunit;

namespace DogRaces.Api.IntegrationTests.Endpoints.WalletEndpoints;

public sealed class WalletEndpointTests : BaseIntegrationTest
{
    public WalletEndpointTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetWalletStatus_WhenCalled_ReturnsDefaultWalletStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/wallet/status");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var walletStatus = await DeserializeResponse<GetWalletStatusResponse>(response);
        
        Assert.Equal(100m, walletStatus.AvailableBalance);
        Assert.Equal(0m, walletStatus.ReservedAmount);
        Assert.Equal(100m, walletStatus.TotalFunds);
        Assert.Contains("Balance: 100", walletStatus.Status);
    }

    [Fact]
    public async Task GetTransactionHistory_WhenCalled_ReturnsInitialTransaction()
    {
        // Act
        var response = await _client.GetAsync("/api/wallet/transactions");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var historyResponse = await DeserializeResponse<GetTransactionHistoryResponse>(response);
        
        Assert.Single(historyResponse.Transactions);
        
        var transaction = historyResponse.Transactions.First();
        Assert.Equal("Payout", transaction.Type);
        Assert.Equal(100m, transaction.Amount);
        Assert.Equal("Initial balance", transaction.Description);
    }

    [Fact]
    public async Task ResetWallet_WithCustomBalance_ShouldUpdateWallet()
    {
        // Arrange
        var resetRequest = new { startingBalance = 250 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/reset", resetRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var resetResponse = await DeserializeResponse<ResetWalletResponse>(response);
        
        Assert.Equal("Wallet reset with balance: 250", resetResponse.Message);
        Assert.Contains("Balance: 250", resetResponse.Status);
    }

    [Fact]
    public async Task ResetWallet_WithNullBalance_ShouldUseDefault()
    {
        // Arrange
        var resetRequest = new { startingBalance = (decimal?)null };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/reset", resetRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var resetResponse = await DeserializeResponse<ResetWalletResponse>(response);
        
        Assert.Equal("Wallet reset with balance: 100", resetResponse.Message);
        Assert.Contains("Balance: 100", resetResponse.Status);
    }

    [Fact]
    public async Task ReserveFunds_WithSufficientBalance_ShouldSucceed()
    {
        // Arrange
        var reserveRequest = new { amount = 50 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/test-reserve", reserveRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var reserveResponse = await DeserializeResponse<ReserveFundsResponse>(response);
        
        Assert.True(reserveResponse.Success);
        Assert.Equal(50, reserveResponse.Amount);
        Assert.NotEqual(Guid.Empty, reserveResponse.TicketId);
        Assert.Contains("Balance: 50", reserveResponse.Status);
        Assert.Contains("Reserved: 50", reserveResponse.Status);
    }

    [Fact]
    public async Task ReserveFunds_WithInsufficientBalance_ShouldFail()
    {
        // Arrange - try to reserve more than available
        var reserveRequest = new { amount = 150.00m };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/test-reserve", reserveRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var reserveResponse = await DeserializeResponse<ReserveFundsResponse>(response);
        
        Assert.False(reserveResponse.Success);
        Assert.Equal(150.00m, reserveResponse.Amount);
    }

    [Fact]
    public async Task ReleaseFunds_ShouldReturnFundsToBalance()
    {
        // Arrange - First reserve some funds
        var reserveRequest = new { amount = 30m };
        var reserveResponse = await _client.PostAsJsonAsync("/api/wallet/test-reserve", reserveRequest);
        var reserveResult = await DeserializeResponse<ReserveFundsResponse>(reserveResponse);
        
        var releaseRequest = new { amount = 30m, ticketId = reserveResult.TicketId };

        // Act
        var response = await _client.PostAsJsonAsync("/api/wallet/test-release", releaseRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var releaseResponseResult = await DeserializeResponse<ReleaseFundsResponse>(response);
        
        Assert.Contains($"Released 30 for ticket {reserveResult.TicketId}", releaseResponseResult.Message);
        Assert.Contains("Balance: 100", releaseResponseResult.Status);
        Assert.Contains("Reserved: 0", releaseResponseResult.Status);
    }

    [Fact]
    public async Task WalletWorkflow_CompleteScenario_ShouldWorkCorrectly()
    {
        // 1. Reset wallet to $200
        var resetRequest = new { startingBalance = 200.00m };
        var resetResponse = await _client.PostAsJsonAsync("/api/wallet/reset", resetRequest);
        resetResponse.EnsureSuccessStatusCode();

        // 2. Reserve $75
        var reserveRequest = new { amount = 75.00m };
        var reserveResponse = await _client.PostAsJsonAsync("/api/wallet/test-reserve", reserveRequest);
        var reserveResult = await DeserializeResponse<ReserveFundsResponse>(reserveResponse);
        
        Assert.True(reserveResult.Success);

        // 3. Check status - should have $125 available, $75 reserved
        var statusResponse = await _client.GetAsync("/api/wallet/status");
        var statusResult = await DeserializeResponse<GetWalletStatusResponse>(statusResponse);
        
        Assert.Equal(125m, statusResult.AvailableBalance);
        Assert.Equal(75m, statusResult.ReservedAmount);
        Assert.Equal(200m, statusResult.TotalFunds);

        // 4. Release $25 of the reserved funds
        var releaseRequest = new { amount = 25.00m, ticketId = reserveResult.TicketId };
        var releaseResponse = await _client.PostAsJsonAsync("/api/wallet/test-release", releaseRequest);
        releaseResponse.EnsureSuccessStatusCode();

        // 5. Final status check - should have $150 available, $50 reserved
        var finalStatusResponse = await _client.GetAsync("/api/wallet/status");
        var finalStatus = await DeserializeResponse<GetWalletStatusResponse>(finalStatusResponse);
        
        Assert.Equal(150m, finalStatus.AvailableBalance);
        Assert.Equal(50m, finalStatus.ReservedAmount);
        Assert.Equal(200m, finalStatus.TotalFunds);

        // 6. Check transaction history has all operations
        var historyResponse = await _client.GetAsync("/api/wallet/transactions");
        var historyResult = await DeserializeResponse<GetTransactionHistoryResponse>(historyResponse);
        
        // Should have: Reset (which creates a new wallet) + Reserve + Release = 3 transactions
        // Note: Reset creates a new wallet instance, so previous transactions are lost
        Assert.Equal(3, historyResult.Transactions.Count());
        
        // Verify the transaction types are correct
        var transactionTypes = historyResult.Transactions.Select(t => t.Type).ToList();
        Assert.Contains("Payout", transactionTypes); // Initial balance from reset
        Assert.Contains("Reserve", transactionTypes); // Reserve operation  
        Assert.Contains("Release", transactionTypes); // Release operation
    }
}
