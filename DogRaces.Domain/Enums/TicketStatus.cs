namespace DogRaces.Domain.Enums;

/// <summary>
/// Status of a betting ticket throughout its lifecycle
/// </summary>
public enum TicketStatus
{
    /// <summary>
    /// Ticket is created but not yet processed
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Ticket was rejected due to validation failures
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Ticket was accepted and bets are active
    /// </summary>
    Success = 3,

    /// <summary>
    /// Ticket is winning and payout processed
    /// </summary>
    Won = 4,

    /// <summary>
    /// Ticket is not winning
    /// </summary>
    Lost = 5
}