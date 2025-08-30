namespace DogRaces.Domain.Enums;

/// <summary>
/// Defines the type of bet placed on a race
/// </summary>
public enum BetType
{
    /// <summary>
    /// Bet on the winner (1st place only)
    /// </summary>
    Winner = 1,
    
    /// <summary>
    /// Bet on a dog finishing in top 2 positions
    /// </summary>
    Top2 = 2,
    
    /// <summary>
    /// Bet on a dog finishing in top 3 positions
    /// </summary>
    Top3 = 3
}
