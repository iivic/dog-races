using DogRaces.Application.Interfaces;
using DogRaces.Domain.Services;

namespace DogRaces.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Test-specific wallet service that maintains state across requests within a test
/// </summary>
public class TestWalletService : IWalletService
{
    private readonly IWalletService _innerService;
    
    public TestWalletService()
    {
        _innerService = new Application.Services.WalletService();
    }

    public Wallet GetWallet() => _innerService.GetWallet();
    public void ResetWallet(decimal startingBalance = 100m) => _innerService.ResetWallet(startingBalance);
    public bool HasSufficientBalance(decimal amount) => _innerService.HasSufficientBalance(amount);
    public bool TryReserveForTicket(decimal amount, Guid ticketId) => _innerService.TryReserveForTicket(amount, ticketId);
    public bool TryCommitForTicket(decimal amount, Guid ticketId) => _innerService.TryCommitForTicket(amount, ticketId);
    public void ReleaseForTicket(decimal amount, Guid ticketId) => _innerService.ReleaseForTicket(amount, ticketId);
    public void AddPayout(decimal amount, Guid ticketId) => _innerService.AddPayout(amount, ticketId);
    public string GetWalletStatus() => _innerService.GetWalletStatus();
    public IReadOnlyList<WalletTransaction> GetTransactionHistory() => _innerService.GetTransactionHistory();
    public decimal GetAvailableBalance() => _innerService.GetAvailableBalance();
    public decimal GetReservedAmount() => _innerService.GetReservedAmount();
    public decimal GetTotalFunds() => _innerService.GetTotalFunds();
}
