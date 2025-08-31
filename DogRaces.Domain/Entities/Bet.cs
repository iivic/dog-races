using DogRaces.Domain.Enums;

namespace DogRaces.Domain.Entities;

/// <summary>
/// Represents a single bet within a ticket
/// </summary>
public class Bet
{
    // Private parameterless constructor for EF Core
    private Bet() { }

    public Bet(long id, long raceId, Guid ticketId, int selection, BetType betType, decimal odds)
    {

        Id = id;
        RaceId = raceId;
        TicketId = ticketId;
        BetType = betType;
        SetSelection(selection);
        SetOdds(odds);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public long Id { get; private set; }
    public long RaceId { get; private set; }
    public Guid TicketId { get; private set; }

    /// <summary>
    /// Selected dog number (1-6)
    /// </summary>
    public int Selection { get; private set; }

    /// <summary>
    /// Type of bet (Winner, Top2, Top3)
    /// </summary>
    public BetType BetType { get; private set; }

    /// <summary>
    /// Odds at the time of bet placement
    /// </summary>
    public decimal Odds { get; private set; }

    /// <summary>
    /// Whether this bet is winning after the race result is published
    /// </summary>
    public bool? IsWinning { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public virtual Race Race { get; private set; } = null!;
    public virtual Ticket Ticket { get; private set; } = null!;

    public void SetSelection(int selection)
    {
        if (selection < 1 || selection > 6)
            throw new ArgumentException("Selection must be between 1 and 6", nameof(selection));

        Selection = selection;
    }

    public void SetOdds(decimal odds)
    {
        if (odds <= 1)
            throw new ArgumentException("Odds must be greater than 1", nameof(odds));

        Odds = odds;
    }

    public void ProcessResult(int[] result)
    {
        IsWinning = CheckIsWinningBet(result);
    }

    /// <summary>
    /// Check if this bet is a winner based on race result
    /// </summary>
    private bool CheckIsWinningBet(int[] result)
    {
        if (result.Length == 0) throw new ArgumentException("Result cannot be empty", nameof(result));
        if (result.Length < 3) throw new ArgumentException("Result must be an array of at least 3 runners", nameof(result));

        return BetType switch
        {
            BetType.Winner => result[0] == Selection,
            BetType.Top2 => result.Take(2).Contains(Selection),
            BetType.Top3 => result.Take(3).Contains(Selection),
            _ => throw new ArgumentException("Invalid bet type", nameof(BetType))
        };
    }
}