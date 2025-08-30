using DogRaces.Domain.Enums;

namespace DogRaces.Domain.Services;

/// <summary>
/// In-memory wallet simulation for tracking balance and transactions
/// </summary>
public class Wallet
{
    /// <summary>
    /// Initialize wallet with starting balance
    /// </summary>
    private Wallet(decimal startingBalance)
    {
        SetBalance(startingBalance);
        ReservedAmount = 0m;
        
        // Log initial balance
        LogTransaction(WalletTransactionType.Payout, startingBalance, "Initial balance", Guid.Empty);
    }

    /// <summary>
    /// Create a new wallet with starting balance
    /// </summary>
    public static Wallet Create(decimal startingBalance = 100m)
    {
        return new Wallet(startingBalance);
    }

    private readonly List<WalletTransaction> _transactions = new();
    
    /// <summary>
    /// Current available balance
    /// </summary>
    public decimal Balance { get; private set; }
    
    /// <summary>
    /// Currently reserved funds for pending bets
    /// </summary>
    public decimal ReservedAmount { get; private set; }
    
    /// <summary>
    /// Total funds (available + reserved)
    /// </summary>
    public decimal TotalFunds => Balance + ReservedAmount;
    
    /// <summary>
    /// Transaction history (read-only)
    /// </summary>
    public IReadOnlyList<WalletTransaction> Transactions => _transactions.AsReadOnly();

    private void SetBalance(decimal balance)
    {
        if (balance < 0)
            throw new ArgumentException("Balance cannot be negative", nameof(balance));

        Balance = balance;
    }

    /// <summary>
    /// Check if sufficient funds available for amount
    /// </summary>
    public bool HasSufficientBalance(decimal amount)
    {
        return Balance >= amount;
    }

    /// <summary>
    /// Reserve funds for a bet (move from balance to reserved)
    /// </summary>
    public bool TryReserve(decimal amount, Guid ticketId)
    {
        if (!HasSufficientBalance(amount))
            return false;

        Balance -= amount;
        ReservedAmount += amount;
        
        LogTransaction(WalletTransactionType.Reserve, amount, 
            $"Reserved funds for ticket {ticketId.ToString()}...", ticketId);
        
        return true;
    }

    /// <summary>
    /// Commit reserved funds (remove from reserved - bet is confirmed)
    /// </summary>
    public bool TryCommit(decimal amount, Guid ticketId)
    {
        if (ReservedAmount < amount)
            return false;

        ReservedAmount -= amount;
        
        LogTransaction(WalletTransactionType.Commit, amount, 
            $"Committed funds for ticket {ticketId.ToString()[..8]}...", ticketId);
        
        return true;
    }

    /// <summary>
    /// Release reserved funds back to balance (bet rejected)
    /// </summary>
    public void Release(decimal amount, Guid ticketId)
    {
        var releaseAmount = Math.Min(amount, ReservedAmount);
        ReservedAmount -= releaseAmount;
        Balance += releaseAmount;
        
        LogTransaction(WalletTransactionType.Release, releaseAmount, 
            $"Released funds for ticket {ticketId.ToString()}...", ticketId);
    }

    /// <summary>
    /// Credit payout directly to balance
    /// </summary>
    public void AddPayout(decimal amount, Guid ticketId)
    {
        Balance += amount;
        
        LogTransaction(WalletTransactionType.Payout, amount, 
            $"Payout for ticket {ticketId.ToString()}...", ticketId);
    }

    /// <summary>
    /// Get wallet status summary
    /// </summary>
    public string GetStatus()
    {
        return $"Balance: {Balance}, Reserved: {ReservedAmount}, Total: {TotalFunds}";
    }

    /// <summary>
    /// Log a transaction
    /// </summary>
    private void LogTransaction(WalletTransactionType type, decimal amount, string description, Guid ticketId)
    {
        _transactions.Add(new WalletTransaction(type, amount, Balance, ReservedAmount, description, ticketId));
    }
}
