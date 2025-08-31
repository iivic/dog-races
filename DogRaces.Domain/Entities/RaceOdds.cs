using DogRaces.Domain.Enums;

namespace DogRaces.Domain.Entities;

/// <summary>
/// Represents the odds for a specific selection in a race
/// </summary>
public class RaceOdds
{
    // Private parameterless constructor for EF Core
    private RaceOdds() { }

    public RaceOdds(long id, long raceId, int selection, decimal odds, BetType betType)
    {
        Id = id;
        RaceId = raceId;
        SetSelection(selection);
        SetOdds(odds);
        BetType = betType;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public long Id { get; private set; }
    public long RaceId { get; private set; }
    
    /// <summary>
    /// Selection number (1-6)
    /// </summary>
    public int Selection { get; private set; }
    
    /// <summary>
    /// Odds value for this selection
    /// </summary>
    public decimal Odds { get; private set; }
    
    /// <summary>
    /// Type of bet (Winner, Top2, Top3)
    /// </summary>
    public BetType BetType { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public virtual Race Race { get; private set; } = null!;

    /// <summary>
    /// Set the odds value
    /// </summary>
    public void SetOdds(decimal newOdds)
    {
        if (newOdds <= 1)
            throw new ArgumentException("Odds must be greater than 1", nameof(newOdds));
        
        Odds = newOdds;
    }

    public void SetSelection(int newSelection)
    {
        if (newSelection < 1 || newSelection > 6)
            throw new ArgumentException("Selection must be between 1 and 6", nameof(newSelection));
        
        Selection = newSelection;
    }

    /// <summary>
    /// Get display string for this selection's odds
    /// </summary>
    public string GetDisplayString()
    {
        return $"Selection {Selection} ({BetType}): {Odds}";
    }
}
