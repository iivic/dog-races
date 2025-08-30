using DogRaces.Domain.Enums;

namespace DogRaces.Domain.Entities;

/// <summary>
/// Represents a betting ticket containing multiple bets
/// </summary>
public class Ticket
{
    // Private parameterless constructor for EF Core
    private Ticket() { }

    public static Ticket Create(decimal totalStake, IReadOnlyList<Bet> bets)
    {
        var ticket = new Ticket();
        ticket.Id = Guid.NewGuid();
        ticket.Status = TicketStatus.Pending;
        
        ticket.SetStake(totalStake);
        ticket.SetBets(bets);
    
        ticket.CreatedAt = DateTimeOffset.UtcNow;
        return ticket;
    }

    public Guid Id { get; private set; }
    public TicketStatus Status { get; private set; }
    
    /// <summary>
    /// Total stake amount (sum of all bet stakes)
    /// </summary>
    public decimal TotalStake { get; private set; }
    
    /// <summary>
    /// Total payout amount (sum of all winning bet payouts)
    /// </summary>
    public decimal? TotalPayout { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<Bet> Bets { get; private set; } = null!;

    private void SetStake(decimal totalStake)
    {
        if (totalStake <= 0.1m)
            throw new ArgumentException("Total stake must be greater than 0.1m", nameof(totalStake));

        TotalStake = totalStake;
    }

    private void SetBets(IReadOnlyList<Bet> bets)
    {
        Bets = bets.ToList();
    }

    /// <summary>
    /// Approve the ticket (change status to Success)
    /// </summary>
    public void Approve()
    {
        if (Status != TicketStatus.Pending)
            throw new InvalidOperationException("Can only approve pending tickets");

        if (!Bets.Any())
            throw new InvalidOperationException("Cannot approve ticket with no bets");

        Status = TicketStatus.Success;
    }

    /// <summary>
    /// Reject the ticket (change status to Rejected)
    /// </summary>
    public void Reject()
    {
        if (Status != TicketStatus.Pending)
            throw new InvalidOperationException("Can only reject pending tickets");

        Status = TicketStatus.Rejected;
    }

    /// <summary>
    /// Process the ticket results (called after race ends)
    /// </summary>
    public void ProcessResult()
    {
        if (Status != TicketStatus.Success)
            throw new InvalidOperationException("Can only process results for successful tickets");

        if (ProcessedAt.HasValue)
            throw new InvalidOperationException("Ticket results have already been processed");

        var isWinning = IsWinning();
        Status = isWinning ? TicketStatus.Won : TicketStatus.Lost;
        ProcessedAt = DateTimeOffset.UtcNow;
        if (!isWinning) return;
        CalculateTotalPayout();
    }

    /// <summary>
    /// Calculate total payout from all winning bets
    /// </summary>
    private void CalculateTotalPayout()
    {
        TotalPayout = Bets.Sum(b => b.Odds) * TotalStake;
    }

    /// <summary>
    /// Check if ticket has any winning bets
    /// </summary>
    public bool IsWinning()
    {
        return Bets.All(b => b.IsWinning ?? false);
    }
}
