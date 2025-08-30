using DogRaces.Domain.Enums;

namespace DogRaces.Domain.Services;

/// <summary>
/// Represents a wallet transaction log entry (in-memory only)
/// </summary>
public class WalletTransaction
{
    internal WalletTransaction(WalletTransactionType type, decimal amount, decimal balanceAfter, decimal reservedAfter, string description, Guid ticketId)
    {
        Id = Guid.NewGuid();
        TicketId = ticketId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        ReservedAfter = reservedAfter;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    
    /// <summary>
    /// Type of transaction
    /// </summary>
    public WalletTransactionType Type { get; private set; }
    
    /// <summary>
    /// Transaction amount
    /// </summary>
    public decimal Amount { get; private set; }
    
    /// <summary>
    /// Available balance after this transaction
    /// </summary>
    public decimal BalanceAfter { get; private set; }
    
    /// <summary>
    /// Reserved amount after this transaction
    /// </summary>
    public decimal ReservedAfter { get; private set; }
    
    /// <summary>
    /// Description of the transaction
    /// </summary>
    public string Description { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }

    public override string ToString()
    {
        return $"{CreatedAt:HH:mm:ss} | {Type} | {Amount} | {Description} | Balance: {BalanceAfter} | Reserved: {ReservedAfter}";
    }
}
