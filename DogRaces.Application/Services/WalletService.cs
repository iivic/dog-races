using DogRaces.Application.Interfaces;
using DogRaces.Domain.Services;

namespace DogRaces.Application.Services;

/// <summary>
/// Service for managing the in-memory wallet system
/// </summary>
public class WalletService : IWalletService
{
    private Wallet _wallet;

    public WalletService()
    {
        // Initialize with default starting balance
        _wallet = Wallet.Create();
    }

    /// <summary>
    /// Get the current wallet instance
    /// </summary>
    public Wallet GetWallet()
    {
        return _wallet;
    }

    /// <summary>
    /// Reset the wallet with a new starting balance
    /// </summary>
    public void ResetWallet(decimal startingBalance = 100m)
    {
        _wallet = Wallet.Create(startingBalance);
    }

    /// <summary>
    /// Check if sufficient funds are available for a bet
    /// </summary>
    public bool HasSufficientBalance(decimal amount)
    {
        return _wallet.HasSufficientBalance(amount);
    }

    /// <summary>
    /// Reserve funds for a ticket
    /// </summary>
    public bool TryReserveForTicket(decimal amount, Guid ticketId)
    {
        return _wallet.TryReserve(amount, ticketId);
    }

    /// <summary>
    /// Commit reserved funds for a confirmed ticket
    /// </summary>
    public bool TryCommitForTicket(decimal amount, Guid ticketId)
    {
        return _wallet.TryCommit(amount, ticketId);
    }

    /// <summary>
    /// Release reserved funds back to balance (ticket rejected)
    /// </summary>
    public void ReleaseForTicket(decimal amount, Guid ticketId)
    {
        _wallet.Release(amount, ticketId);
    }

    /// <summary>
    /// Add payout to balance for winning tickets
    /// </summary>
    public void AddPayout(decimal amount, Guid ticketId)
    {
        _wallet.AddPayout(amount, ticketId);
    }

    /// <summary>
    /// Get wallet status summary
    /// </summary>
    public string GetWalletStatus()
    {
        return _wallet.GetStatus();
    }

    /// <summary>
    /// Get transaction history
    /// </summary>
    public IReadOnlyList<WalletTransaction> GetTransactionHistory()
    {
        return _wallet.Transactions;
    }

    /// <summary>
    /// Get current available balance
    /// </summary>
    public decimal GetAvailableBalance()
    {
        return _wallet.Balance;
    }

    /// <summary>
    /// Get currently reserved amount
    /// </summary>
    public decimal GetReservedAmount()
    {
        return _wallet.ReservedAmount;
    }

    /// <summary>
    /// Get total funds (available + reserved)
    /// </summary>
    public decimal GetTotalFunds()
    {
        return _wallet.TotalFunds;
    }
}