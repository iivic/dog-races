namespace DogRaces.Domain.ValueObjects;

/// <summary>
/// Represents a simulated race result (top 3 positions)
/// </summary>
public record RaceResult(int First, int Second, int Third)
{
    /// <summary>
    /// Check if a runner finished in top 2 positions
    /// </summary>
    public bool IsInTop2(int runnerNumber) => First == runnerNumber || Second == runnerNumber;

    /// <summary>
    /// Check if a runner finished in top 3 positions
    /// </summary>
    public bool IsInTop3(int runnerNumber) => First == runnerNumber || Second == runnerNumber || Third == runnerNumber;
}