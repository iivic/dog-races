namespace DogRaces.Domain.Enums;

/// <summary>
/// Represents the current status of a race
/// </summary>
public enum RaceStatus
{
    Scheduled,      // Created, betting open
    BettingClosed,  // Betting closed, waiting to start
    Running,        // Race in progress
    Finished,       // Race complete, results available
}
