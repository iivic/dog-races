using System.ComponentModel.DataAnnotations.Schema;

namespace DogRaces.Domain.Entities;

/// <summary>
/// Represents a dog race with 6 selections
/// </summary>
public class Race
{
    // Private parameterless constructor for EF Core
    private Race() { }

    public Race(long id, DateTimeOffset startTime, int raceDurationInSeconds)
    {
        Id = id;
        StartTime = startTime;
        SetEndTime(raceDurationInSeconds);
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public long Id { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public bool IsActive { get; private set; }
    
    /// <summary>
    /// Top 3 positions
    /// Set 5 seconds before start, published after race ends
    /// </summary>
    public int[]? Result { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ResultDeterminedAt { get; private set; }
    public DateTimeOffset? ResultPublishedAt { get; private set; }

    // Navigation properties
    public virtual ICollection<Bet> Bets { get; private set; } = null!;
    public virtual ICollection<RaceOdds> RaceOdds { get; private set; } = null!;

    public bool HasResult() => Result != null;
    
    public void SetEndTime(int raceDurationInSeconds)
    {
        EndTime = StartTime.AddSeconds(raceDurationInSeconds);
    }

    /// <summary>
    /// Close the race for betting
    /// </summary>
    public void CloseRaceForBetting(int[] result)
    {
        IsActive = false;
        Result = result;
        ResultDeterminedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Check if the race has started
    /// </summary>
    public bool HasStarted => DateTimeOffset.UtcNow >= StartTime;

    /// <summary>
    /// Check if the race has ended
    /// </summary>
    public bool HasEnded => DateTimeOffset.UtcNow >= EndTime;
}
