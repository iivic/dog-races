using DogRaces.Domain.Services;
using DogRaces.Domain.Enums;
using Xunit;

namespace DogRaces.Domain.UnitTests.Services;

public class WalletTests
{
    [Fact]
    public void Create_WithDefaultBalance_ShouldCreateWalletWith100()
    {
        // Act
        var wallet = Wallet.Create();

        // Assert
        Assert.Equal(100m, wallet.Balance);
        Assert.Equal(0m, wallet.ReservedAmount);
        Assert.Equal(100m, wallet.TotalFunds);
        Assert.Single(wallet.Transactions);
        Assert.Equal(WalletTransactionType.Payout, wallet.Transactions.First().Type);
        Assert.Equal(100m, wallet.Transactions.First().Amount);
    }

    [Fact]
    public void Create_WithCustomBalance_ShouldCreateWalletWithSpecifiedAmount()
    {
        // Arrange
        var customBalance = 250m;

        // Act
        var wallet = Wallet.Create(customBalance);

        // Assert
        Assert.Equal(customBalance, wallet.Balance);
        Assert.Equal(0m, wallet.ReservedAmount);
        Assert.Equal(customBalance, wallet.TotalFunds);
    }

    [Fact]
    public void TryReserve_WithSufficientBalance_ShouldReserveSuccessfully()
    {
        // Arrange
        var wallet = Wallet.Create(100m);
        var ticketId = Guid.NewGuid();
        var reserveAmount = 30m;

        // Act
        var result = wallet.TryReserve(reserveAmount, ticketId);

        // Assert
        Assert.True(result);
        Assert.Equal(70m, wallet.Balance); // 100 - 30
        Assert.Equal(30m, wallet.ReservedAmount);
        Assert.Equal(100m, wallet.TotalFunds); // Total unchanged
    }

    [Fact]
    public void TryReserve_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var wallet = Wallet.Create(50m);
        var ticketId = Guid.NewGuid();
        var reserveAmount = 75m; // More than available

        // Act
        var result = wallet.TryReserve(reserveAmount, ticketId);

        // Assert
        Assert.False(result);
        Assert.Equal(50m, wallet.Balance); // Unchanged
        Assert.Equal(0m, wallet.ReservedAmount); // Unchanged
    }

    [Fact]
    public void Release_WithValidReservation_ShouldReturnFundsToBalance()
    {
        // Arrange
        var wallet = Wallet.Create(100m);
        var ticketId = Guid.NewGuid();
        wallet.TryReserve(40m, ticketId);

        // Act
        wallet.Release(40m, ticketId);

        // Assert
        Assert.Equal(100m, wallet.Balance); // Back to original
        Assert.Equal(0m, wallet.ReservedAmount); // Released
        Assert.Equal(100m, wallet.TotalFunds);
    }

    [Fact]
    public void AddPayout_ShouldIncreaseBalanceAndTotalFunds()
    {
        // Arrange
        var wallet = Wallet.Create(100m);
        var ticketId = Guid.NewGuid();
        var payoutAmount = 50m;

        // Act
        wallet.AddPayout(payoutAmount, ticketId);

        // Assert
        Assert.Equal(150m, wallet.Balance); // 100 + 50
        Assert.Equal(0m, wallet.ReservedAmount); // Unchanged
        Assert.Equal(150m, wallet.TotalFunds); // 100 + 50
    }

    [Fact]
    public void MultipleOperations_ShouldMaintainCorrectBalance()
    {
        // Arrange
        var wallet = Wallet.Create(100m);
        var ticketId1 = Guid.NewGuid();
        var ticketId2 = Guid.NewGuid();

        // Act - Reserve some funds
        wallet.TryReserve(30m, ticketId1);
        wallet.TryReserve(20m, ticketId2);
        
        // Release one reservation and add payout
        wallet.Release(30m, ticketId1);
        wallet.AddPayout(50m, ticketId2);

        // Assert
        Assert.Equal(130m, wallet.Balance); // 100 - 30 - 20 + 30 + 50 = 130
        Assert.Equal(20m, wallet.ReservedAmount); // Still have 20 reserved  
        Assert.Equal(150m, wallet.TotalFunds); // 130 + 20
    }

    [Fact]
    public void GetStatus_ShouldReturnFormattedStatusString()
    {
        // Arrange
        var wallet = Wallet.Create(100m);
        wallet.TryReserve(25m, Guid.NewGuid());

        // Act
        var status = wallet.GetStatus();

        // Assert
        Assert.Contains("Balance: 75", status);
        Assert.Contains("Reserved: 25", status);
        Assert.Contains("Total: 100", status);
    }
}
