namespace DogRaces.Domain.Enums;

/// <summary>
/// Types of wallet transactions for tracking balance changes
/// </summary>
public enum WalletTransactionType
{
    /// <summary>
    /// Funds reserved for pending ticket
    /// </summary>
    Reserve = 1,
    
    /// <summary>
    /// Reserved funds committed (ticket confirmed)
    /// </summary>
    Commit = 2,
    
    /// <summary>
    /// Reserved funds released back to balance (ticket rejected)
    /// </summary>
    Release = 3,
    
    /// <summary>
    /// Winning payout credited to balance
    /// </summary>
    Payout = 4
}
