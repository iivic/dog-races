namespace DogRaces.Domain.Entities;

/// <summary>
/// Global system configuration - single row with specific columns
/// </summary>
public class GlobalConfiguration
{
    public GlobalConfiguration(
        int id,
        decimal minTicketStake,
        decimal maxTicketWin,
        int minNumberOfActiveRounds)
    {
        Id = id;
        MinTicketStake = minTicketStake;
        MaxTicketWin = maxTicketWin;
        MinNumberOfActiveRounds = minNumberOfActiveRounds;
    }

    /// <summary>
    /// Single configuration row
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Minimum stake amount per ticket (e.g., 1)
    /// </summary>
    public decimal MinTicketStake { get; private set; }

    /// <summary>
    /// Maximum amount a ticket can win (e.g., $10,000)
    /// </summary>
    public decimal MaxTicketWin { get; private set; }

    /// <summary>
    /// Minimum number of concurrent active races (default: 7)
    /// </summary>
    public int MinNumberOfActiveRounds { get; private set; }
}