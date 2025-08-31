using DogRaces.Domain.Services;

namespace DogRaces.Application.Interfaces;

/// <summary>
/// Service interface for managing the in-memory wallet system
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Get the current wallet instance
    /// </summary>
    Wallet GetWallet();

    /// <summary>
    /// Reset the wallet with a new starting balance
    /// </summary>
    void ResetWallet(decimal startingBalance = 100m);

    /// <summary>
    /// Check if sufficient funds are available for a bet
    /// </summary>
    bool HasSufficientBalance(decimal amount);

    /// <summary>
    /// Reserve funds for a ticket
    /// </summary>
    bool TryReserveForTicket(decimal amount, Guid ticketId);

    /// <summary>
    /// Commit reserved funds for a confirmed ticket
    /// </summary>
    bool TryCommitForTicket(decimal amount, Guid ticketId);

    /// <summary>
    /// Release reserved funds back to balance (ticket rejected)
    /// </summary>
    void ReleaseForTicket(decimal amount, Guid ticketId);

    /// <summary>
    /// Add payout to balance for winning tickets
    /// </summary>
    void AddPayout(decimal amount, Guid ticketId);

    /// <summary>
    /// Get wallet status summary
    /// </summary>
    string GetWalletStatus();

    /// <summary>
    /// Get transaction history
    /// </summary>
    IReadOnlyList<WalletTransaction> GetTransactionHistory();

    /// <summary>
    /// Get current available balance
    /// </summary>
    decimal GetAvailableBalance();

    /// <summary>
    /// Get currently reserved amount
    /// </summary>
    decimal GetReservedAmount();

    /// <summary>
    /// Get total funds (available + reserved)
    /// </summary>
    decimal GetTotalFunds();
}