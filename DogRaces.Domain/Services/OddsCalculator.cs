namespace DogRaces.Domain.Services;

/// <summary>
/// Utility for calculating odds based on frequency
/// </summary>
public static class OddsCalculator
{
    /// <summary>
    /// Calculate odds based on success count and total simulations
    /// </summary>
    /// <param name="successCount">Number of times outcome occurred</param>
    /// <param name="totalSimulations">Total number of simulations (default: 33 for 100 numbers / 3)</param>
    /// <returns>Calculated odds with minimum floor applied</returns>
    public static decimal GetOdds(int successCount, int totalSimulations = 33)
    {
        if (successCount == 0)
            return 99.0m; // Maximum odds for impossible outcomes

        var probability = (decimal)successCount / totalSimulations;
        var rawOdds = 1 / probability;

        // Apply minimum odds floor to ensure reasonable payouts
        return Math.Max(rawOdds, 1.05m);
    }
}